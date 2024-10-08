using customauthpg.Models;
using customauthpg.Repositories;
using customauthpg.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace customauthpg.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserRepository _userRepository;
    private readonly JwtService _jwtService;
    private readonly PasswordHasher _passwordHasher;

    public AuthController(UserRepository userRepository, JwtService jwtService, PasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = new User
        {
            Username = model.Username,
            Email = model.Email,
            PasswordHash = _passwordHasher.HashPassword(model.Password),
            Role = "User" // Default role
        };

        bool created = await _userRepository.CreateUser(user);
        if (!created)
        {
            return StatusCode(500, "Failed to create user");
        }

        return Ok("User registered successfully");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginModel model)
    {
        var user = await _userRepository.GetUserByUsername(model.Username);
        if (user == null)
        {
            return Unauthorized("Invalid username or password");
        }

        if (!_passwordHasher.VerifyPassword(model.Password, user.PasswordHash))
        {
            return Unauthorized("Invalid username or password");
        }

        var token = _jwtService.GenerateToken(user);
        return Ok(new { Token = token });
    }

    [HttpGet("user")]
    [Authorize]
    public async Task<IActionResult> GetUserInfo()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int id))
        {
            return BadRequest("Invalid user ID.");
        }

        var user = await _userRepository.GetUserById(id);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        return Ok(new
        {
            user.Id,
            user.Username,
            user.Email,
            user.Role
        });
    }
}
