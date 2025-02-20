using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PersonalFinancePlatformAdmin.Application.Dtos.User;
using PersonalFinancePlatformAdmin.Data;
using PersonalFinancePlatformAdmin.Shared.Helpers;

namespace PersonalFinancePlatformAdmin.Presentation.API.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController(
        UserManager<ApplicationUser> _UserManager,
        SignInManager<ApplicationUser> _SignInManager,
        JwtTokenService _JwtTokenService
    ) : ControllerBase
{
    [HttpPost("user-registration")]
    public async Task<IActionResult> UserRegistration([FromForm] RegisterDto register)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        ApplicationUser user = new()
        {
            UserName = register.Username,
            Email = register.Email
        };

        IdentityResult result = await _UserManager.CreateAsync(user, register.Password);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok(new {message = "User Registered Successfully."});
    }

    [HttpPost("user-login")]
    public async Task<IActionResult> UserLogin([FromForm] LoginDto login)
    {
        ApplicationUser? user = await _UserManager.FindByEmailAsync(login.Email);

        if (user is null) return Unauthorized("Invalid User Email");

        var result = await _SignInManager.CheckPasswordSignInAsync(user, login.Password, false);

        if (!result.Succeeded) return Unauthorized("Invalid User Password");

        string token = _JwtTokenService.GenerateToken(user.Id, user.Email!);

        return Ok(new { token });
    }
}
