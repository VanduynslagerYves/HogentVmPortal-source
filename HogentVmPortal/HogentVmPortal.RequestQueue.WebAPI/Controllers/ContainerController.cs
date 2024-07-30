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
    public class ContainerController : ControllerBase
    {
        private readonly ILogger<ContainerController> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        private readonly IContainerRepository _ctRepository;
        private readonly IRequestRepository _requestRepository;

        public ContainerController(ILogger<ContainerController> logger, IServiceScopeFactory serviceScopeFactory, IContainerRepository ctRepository, IRequestRepository requestRepository)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;

            _ctRepository = ctRepository;
            _requestRepository = requestRepository;
        }

        [HttpPost("validate")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> Validate(ContainerCreateRequest request)
        {
            if (request == null) return BadRequest("Invalid request data");

            var nameExists = await _ctRepository.NameExistsAsync(request.Name);

            var isValid = !nameExists;

            return Ok(isValid);
        }

        [HttpPost("create")]
        [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status202Accepted)]
        public IActionResult Create(ContainerCreateRequest createRequest)
        {
            if (createRequest == null) return BadRequest("Invalid request data");

            var taskId = Guid.NewGuid().ToString();

            var request = new Request
            {
                Id = createRequest.Id,
                TimeStamp = createRequest.TimeStamp,
                Name = createRequest.Name,
                RequesterId = createRequest.OwnerId,
                Type = InfraType.Container,
                Status = Status.Pending,
                RequestType = RequestType.Create,
            };

            _requestRepository.Add(request);
            //TODO: save request here in db with taskId and status pending 0

            using (var scope = _serviceScopeFactory.CreateAsyncScope())
            {
                var handler = scope.ServiceProvider.GetRequiredService<ContainerQueueHandler>();
                handler.EnqueueCreateRequest(createRequest);
                //await handler.HandleContainerCreateRequest(request);
            }

            ////TODO: use queued implementation for creation: template ct is locked during cloning
            //Task.Run(async () =>
            //{
            //    await ProcessCreateRequestAsync(request, taskId);
            //});

            return Accepted(new TaskResponse { TaskId = taskId });
        }

        [HttpPost("delete")]
        [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status202Accepted)]
        public IActionResult Delete(ContainerRemoveRequest removeRequest)
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

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var handler = scope.ServiceProvider.GetRequiredService<ContainerQueueHandler>();
                handler.EnqueueRemoveRequest(removeRequest);
            }

            //Task.Run(async () =>
            //{
            //    await ProcessRemoveRequestAsync(request, taskId);
            //});

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
    }
}
