using Microsoft.AspNetCore.Mvc;
using HomeBanking.Contracts.Requests;
using HomeBanking.Contracts.Responses;
using HomeBanking.Core.Interfaces;
using HomeBanking.Infrastructure.Data;

namespace HomeBanking.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly HomeBankingDbContext _context;
    private readonly IJwtService _jwtService;

    public AuthController(HomeBankingDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        // Demo: Hardcoded credentials
        if (request.Email != "demo@bank.com" || request.Password != "Demo123!")
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }

        var user = _context.Users.FirstOrDefault(u => u.Email == request.Email);
        if (user == null)
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }

        var token = _jwtService.GenerateToken(user.Id, user.Email);

        return Ok(new LoginResponse(
            Token: token,
            ExpiresIn: 7200,
            User: new UserDto(user.Id, user.Email, user.FirstName, user.LastName)
        ));
    }
}
