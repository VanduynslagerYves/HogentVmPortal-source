using HogentVmPortal.Shared.DTO;
using HogentVmPortal.Shared.Repositories;
using HogentVmPortal.RequestQueue.WebAPI.Handlers;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace HogentVmPortal.RequestQueue.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VirtualMachineController : ControllerBase
    {
        private readonly ILogger<VirtualMachineController> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        private readonly IVirtualMachineRepository _vmRepository;

        private static ConcurrentDictionary<string, string> _taskStatuses = new ConcurrentDictionary<string, string>();

        public VirtualMachineController(ILogger<VirtualMachineController> logger, IServiceScopeFactory serviceScopeFactory, IVirtualMachineRepository vmRepository)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _vmRepository = vmRepository;
        }

        [HttpPost("validate")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status202Accepted)]
        public async Task<IActionResult> Validate(VirtualMachineCreateRequest request)
        {
            if (request == null) return BadRequest("Invalid request data");

            var nameExists = await _vmRepository.NameExistsAsync(request.Name);

            var isValid = !nameExists;

            return Ok(isValid);
        }

        [HttpPost("create")]
        [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status202Accepted)]
        public IActionResult Create(VirtualMachineCreateRequest request)
        {
            if (request == null) return BadRequest("Invalid request data");

            var taskId = Guid.NewGuid().ToString();

            //Can also do this: Task.Run(async () => await ProcessCreateRequestAsync(request, taskId));
            //this will also immediately return a response while running the task in a background thread.
            //This solution might suffer under heavy load (concurrent create requests), so working with a queue is generally preferred.

            // TODO: this does not need to run on a background thread, this will only enqueue a message to a MQ. So we can just call it

            using (var scope = _serviceScopeFactory.CreateAsyncScope())
            {
                var handler = scope.ServiceProvider.GetRequiredService<VirtualMachineQueueHandler>();
                //await handler.HandleVirtualMachineCreateRequest(request);
                handler.EnqueueCreateRequest(request);
            }

            //Task.Run(async () =>
            //{
            //    await ProcessCreateRequestAsync(request, taskId);
            //});

            //Enqueue a create task
            //_taskQueue.Enqueue(async token =>
            //{
            //    await ProcessCreateRequestAsync(request, taskId, token);
            //});

            return Accepted(new TaskResponse{ TaskId = taskId });
        }

        [HttpPost("delete")]
        [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status202Accepted)]
        public IActionResult Delete(VirtualMachineRemoveRequest request)
        {
            if (request == null) return BadRequest("Invalid request data");

            var taskId = Guid.NewGuid().ToString();

            using (var scope = _serviceScopeFactory.CreateAsyncScope())
            {
                var handler = scope.ServiceProvider.GetRequiredService<VirtualMachineQueueHandler>();
                handler.EnqueueRemoveRequest(request);
                //await handler.HandleVirtualMachineRemoveRequest(request);
            }

            //Task.Run(async () =>
            //{
            //    await ProcessRemoveRequestAsync(request, taskId);
            //});

            //Enqueue a remove task
            //_taskQueue.Enqueue(async token =>
            //{
            //    await ProcessRemoveRequestAsync(request, taskId, token);
            //});

            return Accepted(new TaskResponse{ TaskId = taskId });
        }

        [HttpGet("all")]
        [ProducesResponseType(typeof(IEnumerable<VirtualMachineDTO>), StatusCodes.Status200OK)] //this shows the returned object in the ApiExplorer (e.g. swagger)
        public async Task<IActionResult> GetAll(bool includeUsers = false)
        {
                var vms = await _vmRepository.GetAll(includeUsers);

                return Ok(vms);
        }

        //TODO: mapping to DTO
        [HttpGet("id")]
        [ProducesResponseType(typeof(VirtualMachineDTO), StatusCodes.Status200OK)] //this shows the returned object in the ApiExplorer (e.g. swagger)
        public async Task<IActionResult> GetById(Guid id, bool includeUsers = false)
        {
            try
            {
                var result = await _vmRepository.GetById(id, includeUsers);

                return Ok(result);
            }
            catch(Exception ex)
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
                return Ok(new StatusResponse{ TaskId = taskId, Status = status });
            }

            return NotFound(new StatusResponse{ TaskId = taskId, Status = "Not Found" });
        }

        //private async Task ProcessCreateRequestAsync(VirtualMachineCreateRequest request, string taskId)
        //{
        //    try
        //    {
        //        _taskStatuses[taskId] = "Processing";
        //        _logger.LogInformation("Processing task {taskId}", taskId);

        //        //This task will be executed in a background thread: we need to create a new scope for the handler, so dependencies for the handler can be correctly resolved with DI
        //        using (var scope = _serviceScopeFactory.CreateAsyncScope())
        //        {
        //            var handler = scope.ServiceProvider.GetRequiredService<VirtualMachineQueueHandler>();
        //            //await handler.HandleVirtualMachineCreateRequest(request);
        //            handler.EnqueueCreateRequest(request);
        //        }

        //        _taskStatuses[taskId] = "Completed";
        //        _logger.LogInformation("Completed task {taskId}", taskId);
        //    }
        //    catch (Exception)
        //    {
        //        _taskStatuses[taskId] = "Failed";
        //        _logger.LogError("Failed task {taskId}", taskId);
        //    }
        //}

        //private async Task ProcessRemoveRequestAsync(VirtualMachineRemoveRequest request, string taskId)
        //{
        //    try
        //    {
        //        _taskStatuses[taskId] = "Processing";
        //        _logger.LogInformation("Processing task {taskId}", taskId);

        //        //This task will be executed in a background thread: we need to create a new scope for the handler, so dependencies for the handler can be correctly resolved with DI
        //        using (var scope = _serviceScopeFactory.CreateAsyncScope())
        //        {
        //            var handler = scope.ServiceProvider.GetRequiredService<VirtualMachineQueueHandler>();
        //            handler.EnqueueRemoveRequest(request);
        //            //await handler.HandleVirtualMachineRemoveRequest(request);
        //        }

        //        _taskStatuses[taskId] = "Completed";
        //        _logger.LogInformation("Completed task {taskId}", taskId);
        //    }
        //    catch (Exception)
        //    {
        //        _taskStatuses[taskId] = "Failed";
        //        _logger.LogError("Failed task {taskId}", taskId);
        //    }
        //}
    }
}
