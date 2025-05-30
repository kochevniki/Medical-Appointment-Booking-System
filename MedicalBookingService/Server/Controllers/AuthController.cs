using MedicalBookingService.Client.Components;
using MedicalBookingService.Server.Models;
using MedicalBookingService.Server.Models.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MedicalBookingService.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager;

    public AuthController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null) return Unauthorized("Invalid credentials");

        var result = await _signInManager.PasswordSignInAsync(
            user.UserName, model.Password, isPersistent: false, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            return Ok();
        }
       
        return Unauthorized();
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
}
