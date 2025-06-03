using MedicalBookingService.Server.Data;
using MedicalBookingService.Server.Models;
using MedicalBookingService.Shared.Models;
using MedicalBookingService.Server.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
namespace MedicalBookingService.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDbContext _db;
        private readonly EmailService _emailService;

        public AccountController(UserManager<AppUser> userManager, ApplicationDbContext db, EmailService emailService)
        {
            _userManager = userManager;
            _db = db;
            _emailService = emailService;
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Signup(PatientSignupModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
                return Conflict("An account with this email already exists.");

            var user = new AppUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true,
                Role = "Patient"
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            await _userManager.AddToRoleAsync(user, "Patient");

            var profile = new PatientProfile
            {
                AppUserId = user.Id,
                FirstName = model.FirstName,
                LastName = model.LastName,
                SSN = model.SSN,
                DateOfBirth = model.DateOfBirth,
                Address = model.Address,
                GovernmentIdUrl = model.GovernmentIdUrl,
                InsuranceCardUrl = model.InsuranceCardUrl,
            };

            _db.PatientProfiles.Add(profile);
            await _db.SaveChangesAsync();

            //// Generate email confirmation token
            //var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            //var confirmationLink = Url.Action(
            //    "ConfirmEmail", "Account",
            //    new { userId = user.Id, token },
            //    protocol: HttpContext.Request.Scheme);

            // Send confirmation email
            //var body = $"Please confirm your email by clicking this link: <a href='{confirmationLink}'>Confirm Email</a>";
            //await _emailService.SendEmailAsync(user.Email, "Confirm your email", body);

            //return Ok("Signup successful. Please check your email to confirm your account.");
            return Ok();
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return BadRequest("Invalid user ID");

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
                return Content("Email confirmed successfully. You can now log in.");
            else
                return BadRequest("Email confirmation failed.");
        }

    }
}
