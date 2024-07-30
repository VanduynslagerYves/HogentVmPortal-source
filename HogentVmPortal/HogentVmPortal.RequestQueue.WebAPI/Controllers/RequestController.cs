using HogentVmPortal.Shared.Model;
using HogentVmPortal.Shared.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace HogentVmPortal.RequestQueue.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RequestController : ControllerBase
    {
        private readonly ILogger<RequestController> _logger;
        private readonly IRequestRepository _requestRepository;

        public RequestController(ILogger<RequestController> logger, IRequestRepository requestRepository)
        {
            _logger = logger;
            _requestRepository = requestRepository;
        }

        //[HttpPost("update")]
        //[ProducesResponseType(typeof(OkResult), StatusCodes.Status200OK)]
        //public IActionResult UpdateRequest(Request request)
        //{
        //    if (request == null) return BadRequest("Invalid request data");

        //    _requestRepository.Update(request);
        //    return Ok();
        //}

        [HttpGet("id")]
        [ProducesResponseType(typeof(Request), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var request = await _requestRepository.GetById(id);
                if (request == null) return Ok();

                return Ok(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            return NotFound(id);
        }
    }
}
