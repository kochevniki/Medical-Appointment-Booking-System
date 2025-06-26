using MedicalBookingService.Server.Data;
using MedicalBookingService.Server.Models;
using MedicalBookingService.Shared.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalBookingService.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BoxTokenController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BoxTokenController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<BoxTokenDto>> GetTokens()
        {
            var token = await _context.BoxTokens.FirstOrDefaultAsync();

            if (token == null)
            {
                return NotFound("No Box tokens found.");
            }

            var tokenDto = new BoxTokenDto
            {
                RefreshToken = token.RefreshToken,
                AccessToken = token.AccessToken
            };

            return Ok(tokenDto);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateBoxTokens([FromBody] BoxTokenDto tokenDto)
        {
            var tokenEntry = await _context.BoxTokens.FirstOrDefaultAsync();

            if (tokenEntry == null)
            {
                tokenEntry = new BoxToken
                {
                    RefreshToken = tokenDto.RefreshToken,
                    AccessToken = tokenDto.AccessToken,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.BoxTokens.Add(tokenEntry);
            }
            else
            {
                tokenEntry.RefreshToken = tokenDto.RefreshToken;
                tokenEntry.AccessToken = tokenDto.AccessToken;
                tokenEntry.UpdatedAt = DateTime.UtcNow;
                _context.BoxTokens.Update(tokenEntry);
            }

            await _context.SaveChangesAsync();
            return Ok("Tokens updated successfully");
        }
    }
}
