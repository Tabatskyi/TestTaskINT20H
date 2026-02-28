using Microsoft.AspNetCore.Mvc;
using TestTaskINT20H.Application.Auth.DTOs;
using TestTaskINT20H.Application.Auth.Services;
using TestTaskINT20H.Application.Shared;

namespace TestTaskINT20H.Presentation.Controllers;

[ApiController]
[Route("auth")]
[Produces("application/json")]
public sealed class AuthController(AuthApplicationService authService) : ControllerBase
{
    private readonly AuthApplicationService _authService = authService ?? throw new ArgumentNullException(nameof(authService));

    /// <summary>
    /// Authenticate with username and password, returns a JWT bearer token
    /// </summary>
    /// <param name="dto">Login credentials</param>
    /// <returns>JWT token and expiry</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(TokenDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public IActionResult Login([FromBody] LoginDto dto)
    {
        var result = _authService.Login(dto);
        if (result is null)
            return Unauthorized(new ErrorResponse { Error = "Invalid username or password." });

        return Ok(result);
    }
}
