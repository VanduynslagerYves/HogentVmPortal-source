using HogentVmPortal.Shared.DTO;
using HogentVmPortalWebAPI.Handlers;
using HogentVmPortalWebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace HogentVmPortalWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VirtualMachineController : ControllerBase
    {
        private readonly ILogger<VirtualMachineController> _logger;

        private readonly IBackgroundTaskQueue _taskQueue;
        private static ConcurrentDictionary<string, string> _taskStatuses = new ConcurrentDictionary<string, string>();

        public VirtualMachineController(IBackgroundTaskQueue taskQueue, ILogger<VirtualMachineController> logger)
        {
            _taskQueue = taskQueue;
            _logger = logger;
        }

        [HttpPost("createvm")]
        public IActionResult CreateVm([FromBody] VirtualMachineCreateRequest request)
        {
            if (request == null) return BadRequest("Invalid request data");

            var taskId = Guid.NewGuid().ToString();

            //Can also do this: Task.Run(async () => await ProcessCreateRequestAsync(request, taskId));
            //this will also immediately return a response while running the task in a background thread.
            //This solution might suffer under heavy load (concurrent create requests), so working with a queue is generally preferred.

            _taskQueue.Enqueue(async token =>
            {
                await ProcessCreateRequestAsync(request, taskId, token);
            });

            return Accepted(new { TaskId = taskId });
        }

        [HttpPost("deletevm")]
        public IActionResult DeleteVm([FromBody] VirtualMachineRemoveRequest request)
        {
            if (request == null) return BadRequest("Invalid request data");

            var taskId = Guid.NewGuid().ToString();

            _taskQueue.Enqueue(async token =>
            {
                await ProcessRemoveRequestAsync(request, taskId, token);
            });

            return Accepted(new { TaskId = taskId });
        }

        [HttpGet("status/{taskId}")]
        public IActionResult GetStatus(string taskId)
        {
            if (_taskStatuses.TryGetValue(taskId, out var status))
            {
                return Ok(new { TaskId = taskId, Status = status });
            }

            return NotFound(new { TaskId = taskId, Status = "Not Found" });
        }

        private async Task ProcessCreateRequestAsync(VirtualMachineCreateRequest request, string taskId, CancellationToken token)
        {
            try
            {
                _taskStatuses[taskId] = "Processing";
                _logger.LogInformation("Processing task {taskId}", taskId);

                // Resolve VirtualMachineHandler through DI (TODO: check for DI in constructor)
                using (var scope = HttpContext.RequestServices.CreateScope())
                {
                    var handler = scope.ServiceProvider.GetRequiredService<VirtualMachineHandler>();
                    await handler.HandleVirtualMachineCreateRequest(request);
                }

                _taskStatuses[taskId] = "Completed";
                _logger.LogInformation("Completed task {taskId}", taskId);
            }
            catch (OperationCanceledException)
            {
                _taskStatuses[taskId] = "Cancelled";
                _logger.LogInformation("Cancelled task {taskId}", taskId);
            }
            catch (Exception)
            {
                _taskStatuses[taskId] = "Failed";
                _logger.LogInformation("Failed task {taskId}", taskId);
            }
        }

        private async Task ProcessRemoveRequestAsync(VirtualMachineRemoveRequest request, string taskId, CancellationToken token)
        {
            try
            {
                _taskStatuses[taskId] = "Processing";
                _logger.LogInformation("Processing task {taskId}", taskId);

                // Resolve VirtualMachineHandler through DI
                using (var scope = HttpContext.RequestServices.CreateScope())
                {
                    var handler = scope.ServiceProvider.GetRequiredService<VirtualMachineHandler>();
                    await handler.HandleVirtualMachineRemoveRequest(request);
                }

                _taskStatuses[taskId] = "Completed";
                _logger.LogInformation("Completed task {taskId}", taskId);
            }
            catch (OperationCanceledException)
            {
                _taskStatuses[taskId] = "Cancelled";
                _logger.LogInformation("Cancelled task {taskId}", taskId);
            }
            catch (Exception)
            {
                _taskStatuses[taskId] = "Failed";
                _logger.LogInformation("Failed task {taskId}", taskId);
            }
        }
    }
}
