using HogentVmPortal.Shared.DTO;
using HogentVmPortal.Shared.Repositories;
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
        private readonly IServiceScopeFactory _serviceScopeFactory;

        private readonly IContainerRepository _ctRepository;

        private static ConcurrentDictionary<string, string> _taskStatuses = new ConcurrentDictionary<string, string>();

        public ContainerController(ILogger<ContainerController> logger, IServiceScopeFactory serviceScopeFactory, IContainerRepository ctRepository)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _ctRepository = ctRepository;
        }

        [HttpPost("validate")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status202Accepted)]
        public async Task<IActionResult> Validate(ContainerCreateRequest request)
        {
            if (request == null) return BadRequest("Invalid request data");

            var nameExists = await _ctRepository.NameExistsAsync(request.Name);

            var isValid = !nameExists;

            return Ok(isValid);
        }

        [HttpPost("create")]
        [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status202Accepted)]
        public IActionResult Create(ContainerCreateRequest request)
        {
            if (request == null) return BadRequest("Invalid request data");

            var taskId = Guid.NewGuid().ToString();

            //TODO: save request here in db with taskId and status pending 0

            //TODO: use queued implementation for creation: template ct is locked during cloning
            Task.Run(async () =>
            {
                await ProcessCreateRequestAsync(request, taskId);
            });

            return Accepted(new TaskResponse { TaskId = taskId });
        }

        [HttpPost("delete")]
        [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status202Accepted)]
        public IActionResult Delete(ContainerRemoveRequest request)
        {
            if (request == null) return BadRequest("Invalid request data");

            var taskId = Guid.NewGuid().ToString();

            Task.Run(async () =>
            {
                await ProcessRemoveRequestAsync(request, taskId);
            });

            return Accepted(new TaskResponse { TaskId = taskId });
        }

        [HttpGet("all")]
        [ProducesResponseType(typeof(IEnumerable<ContainerDTO>), StatusCodes.Status200OK)] //this shows the returned object in the ApiExplorer (e.g. swagger)
        public async Task<IActionResult> GetAll(bool includeUsers = false)
        {
            var cts = await _ctRepository.GetAll(includeUsers);

            return Ok(cts);
        }

        //TODO: mapping to DTO
        [HttpGet("id")]
        [ProducesResponseType(typeof(ContainerDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetById(Guid id, bool includeUsers = false)
        {
            try
            {
                var result = await _ctRepository.GetById(id, includeUsers);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            return NotFound(id);
        }

        [HttpGet("status/{taskId}")]
        [ProducesResponseType(typeof(StatusResponse), StatusCodes.Status200OK)]
        public IActionResult GetStatus(string taskId)
        {
            if (_taskStatuses.TryGetValue(taskId, out var status))
            {
                return Ok(new StatusResponse { TaskId = taskId, Status = status });
            }

            return NotFound(new StatusResponse { TaskId = taskId, Status = "Not Found" });
        }

        //This task will be executed in a background thread: we need to create a new scope for the task, so dependencies for the task can be correctly resolved with DI
        private async Task ProcessCreateRequestAsync(ContainerCreateRequest request, string taskId)
        {
            try
            {
                //TODO: update request here in db with the taskId and updated status
                _taskStatuses[taskId] = "Processing";
                _logger.LogInformation("Processing task {taskId}", taskId);

                using (var scope = _serviceScopeFactory.CreateScope())
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
                _logger.LogError("Failed task {taskId}", taskId);
            }
        }

        //This task will be executed in a background thread: we need to create a new scope for the task, so dependencies for the task can be correctly resolved with DI
        private async Task ProcessRemoveRequestAsync(ContainerRemoveRequest request, string taskId)
        {
            try
            {
                _taskStatuses[taskId] = "Processing";
                _logger.LogInformation("Processing task {taskId}", taskId);

                using (var scope = _serviceScopeFactory.CreateScope())
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
                _logger.LogError("Failed task {taskId}", taskId);
            }
        }
    }
}
