using MedicalBookingService.Server.Models;
using MedicalBookingService.Server.Models.DTOs;
using MedicalBookingService.Server.Services;
using MedicalBookingService.Shared.Models;
using MedicalBookingService.Shared.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace MedicalBookingService.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager;
    private readonly AuthService _authService;
    private readonly EmailService _emailService;

    public AuthController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, AuthService authService, EmailService emailService)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _authService = authService;
        _emailService = emailService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null) return Unauthorized("User with this is email does not exist in the system.");

        if(!user.EmailConfirmed) return Unauthorized("Email not confirmed. Please check your inbox.");

        var result = await _signInManager.PasswordSignInAsync(
            user.UserName, model.Password, isPersistent: false, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            return Ok();
        }
       
        return Unauthorized("Invalid credentials.");
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        var cookieOptions = new CookieOptions
        {
            Path = "/",
            Secure = true
        };
        Response.Cookies.Delete(".AspNetCore.Identity.Application", cookieOptions);
        return Ok();
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            return Ok(); // Don't reveal user existence

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        //var callbackUrl = Url.Action("ResetPassword", "Auth", new { token, email = user.Email }, Request.Scheme);

        var encodedToken = WebUtility.UrlEncode(token);
        var frontendBaseUrl = "https://localhost:8080"; // Ideally pulled from config
        var callbackUrl = $"{frontendBaseUrl}/reset-password?token={encodedToken}&email={user.Email}";

        await _emailService.SendEmailAsync(user.Email, "Reset Password",
            $"Reset your password by clicking [here]({callbackUrl})");

        return Ok();
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null) return Ok();

        var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
        if (result.Succeeded) return Ok();

        return BadRequest(result.Errors);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var user = await _authService.GetUserByIdAsync(userId);
        var roles = await _authService.GetUserRolesAsync(user);

        var userInfo = new UserInfo
        {
            Id = user.Id,
            Email = user.Email,
            Roles = roles
        };

        return Ok(userInfo);
    }
}
