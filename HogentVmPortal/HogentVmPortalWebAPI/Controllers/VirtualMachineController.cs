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
        private readonly IBackgroundTaskQueue _taskQueue;
        private static ConcurrentDictionary<string, string> _taskStatuses = new ConcurrentDictionary<string, string>();

        public VirtualMachineController(IBackgroundTaskQueue taskQueue)
        {
            _taskQueue = taskQueue;
        }

        [HttpPost("createvm")]
        public IActionResult CreateVm([FromBody] VirtualMachineCreateRequest request)
        {
            var taskId = Guid.NewGuid().ToString();
            _taskStatuses[taskId] = "Processing";

            _taskQueue.QueueBackgroundWorkItem(async token =>
            {
                await ProcessCreateRequestAsync(request, taskId, token);
                _taskStatuses[taskId] = "Completed";
            });

            return Accepted(new { TaskId = taskId });
        }

        [HttpPost("deletevm")]
        public IActionResult DeleteVm([FromBody] VirtualMachineRemoveRequest request)
        {
            var taskId = Guid.NewGuid().ToString();
            _taskStatuses[taskId] = "Processing";

            _taskQueue.QueueBackgroundWorkItem(async token =>
            {
                await ProcessRemoveRequestAsync(request, taskId, token);
                _taskStatuses[taskId] = "Completed";
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
                // Resolve VirtualMachineHandler through DI (TODO: check for DI in constructor)
                using (var scope = HttpContext.RequestServices.CreateScope())
                {
                    var handler = scope.ServiceProvider.GetRequiredService<VirtualMachineHandler>();
                    await handler.HandleVirtualMachineCreateRequest(request);
                }

                _taskStatuses[taskId] = "Completed";
            }
            catch (OperationCanceledException)
            {
                _taskStatuses[taskId] = "Cancelled";
            }
            catch (Exception)
            {
                _taskStatuses[taskId] = "Failed";
            }
        }

        private async Task ProcessRemoveRequestAsync(VirtualMachineRemoveRequest request, string taskId, CancellationToken token)
        {
            try
            {
                // Resolve VirtualMachineHandler through DI
                using (var scope = HttpContext.RequestServices.CreateScope())
                {
                    var handler = scope.ServiceProvider.GetRequiredService<VirtualMachineHandler>();
                    await handler.HandleVirtualMachineRemoveRequest(request);
                }

                _taskStatuses[taskId] = "Completed";
            }
            catch (OperationCanceledException)
            {
                _taskStatuses[taskId] = "Cancelled";
            }
            catch (Exception)
            {
                _taskStatuses[taskId] = "Failed";
            }
        }
    }
}
