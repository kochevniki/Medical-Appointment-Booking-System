using MedicalBookingService.Server.Services;
using MedicalBookingService.Shared.Services;
using Microsoft.AspNetCore.Mvc;

namespace MedicalBookingService.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : ControllerBase
    {
        private readonly FileService _fileService;
        private readonly ILogger<AuthService> _logger;

        public FileController(FileService fileService, ILogger<AuthService> logger)
        {
            _fileService = fileService;
            _logger = logger;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file, [FromForm] string userId)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            string pathAndFileName = $"patients/{userId}/{file.FileName}";

            var url = await _fileService.UploadAsync(pathAndFileName, file.OpenReadStream(), file.ContentType);
            return Ok(new { url });
        }

        [HttpGet("download/{fileName}")]
        public async Task<IActionResult> Download(string fileName)
        {
            var stream = await _fileService.DownloadAsync(fileName);
            if (stream == null)
                return NotFound();

            return File(stream, "application/octet-stream", fileName);
        }

        [HttpDelete("delete/{fileName}")]
        public async Task<IActionResult> Delete(string fileName)
        {
            await _fileService.DeleteAsync(fileName);
            return Ok();
        }
    }
}
