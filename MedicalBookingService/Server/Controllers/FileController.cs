using MedicalBookingService.Server.Services;
using MedicalBookingService.Shared.Services;
using Microsoft.AspNetCore.Authorization;
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

        [HttpGet("preview/{fileId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<string>> GetPreviewUrl(string fileId)
        {
            try
            {
                var url = await _fileService.GetPreviewUrlAsync(fileId);
                return Ok(url);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("delete/{fileName}")]
        public async Task<IActionResult> Delete(string fileName)
        {
            await _fileService.DeleteAsync(fileName);
            return Ok();
        }
    }
}
