using HogentVmPortal.Shared.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace HogentVmPortal.RequestQueue.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ValidateController : ControllerBase
    {
        private readonly ILogger<ValidateController> _logger;

        private readonly IVirtualMachineRepository _vmRepository;
        private readonly IContainerRepository _ctRepository;
        private readonly IRequestRepository _requestRepository;

        public ValidateController(ILogger<ValidateController> logger, IVirtualMachineRepository vmRepository, IContainerRepository ctRepository, IRequestRepository requestRepository)
        {
            _logger = logger;
            _vmRepository = vmRepository;
            _ctRepository = ctRepository;
            _requestRepository = requestRepository;
        }

        //we also need to check if there are any pending operations with this name.
        // Add the request to the database, with status: Pending, Complete or Active, Failed
        // pass the request record id on create or delete.
        // ON statuschange (created or deleted), call the RequestQueue.WebAPI to update the request record.
        // then also check the request tables for existing names, only for Pending and Active
        [HttpGet("name")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> UniqueName(string name)
        {
            if (string.IsNullOrEmpty(name)) return BadRequest("Invalid data");

            var ctNameExists = await _ctRepository.NameExistsAsync(name);
            var vmNameExists = await _vmRepository.NameExistsAsync(name);
            var requestNameExists = await _requestRepository.NameExistsAsync(name);

            var isValid = !(ctNameExists || vmNameExists || requestNameExists);

            return Ok(isValid);
        }
    }
}
