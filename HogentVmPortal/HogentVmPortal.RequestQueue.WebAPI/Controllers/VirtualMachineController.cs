using HogentVmPortal.Shared.DTO;
using HogentVmPortal.Shared.Repositories;
using HogentVmPortal.RequestQueue.WebAPI.Handlers;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using HogentVmPortal.Shared.Model;

namespace HogentVmPortal.RequestQueue.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VirtualMachineController : ControllerBase
    {
        private readonly ILogger<VirtualMachineController> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        private readonly IVirtualMachineRepository _vmRepository;
        private readonly IRequestRepository _requestRepository;

        public VirtualMachineController(ILogger<VirtualMachineController> logger, IServiceScopeFactory serviceScopeFactory, IVirtualMachineRepository vmRepository, IRequestRepository requestRepository)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;

            _vmRepository = vmRepository;
            _requestRepository = requestRepository;
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
        public IActionResult Create(VirtualMachineCreateRequest createRequest)
        {
            if (createRequest == null) return BadRequest("Invalid request data");

            var taskId = Guid.NewGuid().ToString();

            var request = new Request
            {
                Id = createRequest.Id,
                TimeStamp = createRequest.TimeStamp,
                Name = createRequest.Name,
                RequesterId = createRequest.OwnerId,
                Type = InfraType.VirtualMachine,
                Status = Status.Pending,
                RequestType = RequestType.Create,
            };

            _requestRepository.Add(request);
            //Can also do this: Task.Run(async () => await ProcessCreateRequestAsync(request, taskId));
            //this will also immediately return a response while running the task in a background thread.
            //This solution might suffer under heavy load (concurrent create requests), so working with a queue is generally preferred.

            // TODO: this does not need to run on a background thread, this will only enqueue a message to a MQ. So we can just call it

            using (var scope = _serviceScopeFactory.CreateAsyncScope())
            {
                var handler = scope.ServiceProvider.GetRequiredService<VirtualMachineQueueHandler>();
                handler.EnqueueCreateRequest(createRequest);
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
        public IActionResult Delete(VirtualMachineRemoveRequest removeRequest)
        {
            if (removeRequest == null) return BadRequest("Invalid request data");

            var taskId = Guid.NewGuid().ToString();

            var request = new Request
            {
                Id = removeRequest.Id,
                TimeStamp = removeRequest.TimeStamp,
                Name = removeRequest.Name,
                RequesterId = removeRequest.OwnerId,
                Type = InfraType.Container,
                Status = Status.Pending,
                RequestType = RequestType.Remove,
            };

            _requestRepository.Add(request);

            using (var scope = _serviceScopeFactory.CreateAsyncScope())
            {
                var handler = scope.ServiceProvider.GetRequiredService<VirtualMachineQueueHandler>();
                handler.EnqueueRemoveRequest(removeRequest);
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
    }
}
