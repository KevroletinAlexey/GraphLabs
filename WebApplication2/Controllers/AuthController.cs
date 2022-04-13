using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Auth;

namespace WebApplication2.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly UserService _userService;

    public AuthController(UserService userService)
    {
        _userService = userService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody]LoginRequest request)
    {
        var response = await _userService.Authenticate(request);

        if (response == null)
            return BadRequest(new { message = "Email or password is incorrect" });

        return Ok(response);
    }
        
    [HttpPost("renew")]
    public async Task<IActionResult> Renew()
    {
        var response = await _userService.Renew(User);

        if (response == null)
            return BadRequest(new { message = "Invalid token" });

        return Ok(response);
    }
}