using HogentVmPortal.Shared.DTO;
using HogentVmPortalWebAPI.Handlers;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace HogentVmPortalWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContainerController : ControllerBase
    {
        private readonly ILogger<ContainerController> _logger;
        private readonly IServiceProvider _serviceProvider;

        private static ConcurrentDictionary<string, string> _taskStatuses = new ConcurrentDictionary<string, string>();

        public ContainerController(ILogger<ContainerController> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        [HttpPost("create")]
        public IActionResult Create(ContainerCreateRequest request)
        {
            if (request == null) return BadRequest("Invalid request data");

            var taskId = Guid.NewGuid().ToString();

            //TODO: use queued implementation for creation: template ct is locked during cloning
            Task.Run(async () =>
            {
                await ProcessCreateRequestAsync(request, taskId);
            });

            return Accepted(new { TaskId = taskId });
        }

        [HttpPost("delete")]
        public IActionResult Delete(ContainerRemoveRequest request)
        {
            if (request == null) return BadRequest("Invalid request data");

            var taskId = Guid.NewGuid().ToString();

            Task.Run(async () =>
            {
                await ProcessRemoveRequestAsync(request, taskId);
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

        //This task will be executed in a background thread: we need to create a new scope for the task, so dependencies for the task can be correctly resolved with DI
        private async Task ProcessCreateRequestAsync(ContainerCreateRequest request, string taskId)
        {
            try
            {
                _taskStatuses[taskId] = "Processing";
                _logger.LogInformation("Processing task {taskId}", taskId);

                using (var scope = _serviceProvider.CreateScope())
                {
                    var handler = scope.ServiceProvider.GetRequiredService<ContainerHandler>();
                    await handler.HandleContainerCreateRequest(request);
                }

                _taskStatuses[taskId] = "Completed";
                _logger.LogInformation("Completed task {taskId}", taskId);
            }
            catch (Exception)
            {
                _taskStatuses[taskId] = "Failed";
                _logger.LogInformation("Failed task {taskId}", taskId);
            }
        }

        //This task will be executed in a background thread: we need to create a new scope for the task, so dependencies for the task can be correctly resolved with DI
        private async Task ProcessRemoveRequestAsync(ContainerRemoveRequest request, string taskId)
        {
            try
            {
                _taskStatuses[taskId] = "Processing";
                _logger.LogInformation("Processing task {taskId}", taskId);

                using (var scope = _serviceProvider.CreateScope())
                {
                    var handler = scope.ServiceProvider.GetRequiredService<ContainerHandler>();
                    await handler.HandleContainerRemoveRequest(request);
                }

                _taskStatuses[taskId] = "Completed";
                _logger.LogInformation("Completed task {taskId}", taskId);
            }
            catch (Exception)
            {
                _taskStatuses[taskId] = "Failed";
                _logger.LogInformation("Failed task {taskId}", taskId);
            }
        }
    }
}
