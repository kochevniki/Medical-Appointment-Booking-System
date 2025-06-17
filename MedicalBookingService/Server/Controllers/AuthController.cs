using MedicalBookingService.Server.Models;
using MedicalBookingService.Server.Models.DTOs;
using MedicalBookingService.Server.Services;
using MedicalBookingService.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MedicalBookingService.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager;
    private readonly AuthService _authService;

    public AuthController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, AuthService authService)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null) return Unauthorized("User with this is email does not exist in the system.");

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
