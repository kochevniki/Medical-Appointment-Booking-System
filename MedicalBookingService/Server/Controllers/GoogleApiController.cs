using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace MedicalBookingService.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GoogleApiController : ControllerBase
    {
        private readonly IConfiguration _config;

        public GoogleApiController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("key")]
        public IActionResult GetPlacesApiKey()
        {
            var key = _config["Google:PlacesApiKey"];
            if (string.IsNullOrEmpty(key)) return NotFound();
            return Ok(key);
        }
    }
}
