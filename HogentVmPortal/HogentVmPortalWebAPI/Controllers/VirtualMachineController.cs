using HogentVmPortal.Shared.DTO;
using HogentVmPortalWebAPI.Handlers;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace HogentVmPortalWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VirtualMachineController : ControllerBase
    {
        private readonly ILogger<VirtualMachineController> _logger;

        //IServiceScopeFactory to ensure that scoped dependencies are resolved correctly, even when there's no active HTTP context.
        //This is mandatory to resolve dependencies for the 2nd and later tasks in the taskqueue
        //for reference: IServiceProvider only works within the current HTTP context (phew)
        private readonly IServiceScopeFactory _scopeFactory;

        private readonly IBackgroundTaskQueue _taskQueue;
        private static ConcurrentDictionary<string, string> _taskStatuses = new ConcurrentDictionary<string, string>();

        public VirtualMachineController(IBackgroundTaskQueue taskQueue, ILogger<VirtualMachineController> logger, IServiceScopeFactory scopeFactory)
        {
            _taskQueue = taskQueue;
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        [HttpPost("createvm")]
        public IActionResult CreateVm([FromBody] VirtualMachineCreateRequest request)
        {
            if (request == null) return BadRequest("Invalid request data");

            var taskId = Guid.NewGuid().ToString();

            //Can also do this: Task.Run(async () => await ProcessCreateRequestAsync(request, taskId));
            //this will also immediately return a response while running the task in a background thread.
            //This solution might suffer under heavy load (concurrent create requests), so working with a queue is generally preferred.

            //Enqueue a create task
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

        //This task will be saved in a queue, so we need to create a new scope for each task, so dependencies for the task can be correctly resolved with DI
        private async Task ProcessCreateRequestAsync(VirtualMachineCreateRequest request, string taskId, CancellationToken token)
        {
            try
            {
                _taskStatuses[taskId] = "Processing";
                _logger.LogInformation("Processing task {taskId}", taskId);

                // Resolve VirtualMachineHandler through DI (without the need of an HTTP context)
                using(var scope = _scopeFactory.CreateScope())
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

        //This task will be saved in a queue, so we need to create a new scope for each task, so dependencies for the task can be correctly resolved with DI
        private async Task ProcessRemoveRequestAsync(VirtualMachineRemoveRequest request, string taskId, CancellationToken token)
        {
            try
            {
                _taskStatuses[taskId] = "Processing";
                _logger.LogInformation("Processing task {taskId}", taskId);

                // Resolve VirtualMachineHandler through DI (without the need of an HTTP context)
                using (var scope = _scopeFactory.CreateScope())
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
