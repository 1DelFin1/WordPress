using Inpad.Api.Data;
using Inpad.Api.DTOs;
using Inpad.Api.Models;
using Inpad.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Inpad.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AppDbContext db, TokenService tokenService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto dto)
    {
        if (await db.Users.AnyAsync(u => u.Email == dto.Email))
            return Conflict(new { message = "Пользователь с таким email уже существует." });

        var user = new User
        {
            Email = dto.Email.ToLower().Trim(),
            Name = dto.Name.Trim(),
            PasswordHash = HashPassword(dto.Password),
            Role = UserRole.Viewer
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return Ok(BuildResponse(user));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email.ToLower().Trim());

        if (user is null || !VerifyPassword(dto.Password, user.PasswordHash))
            return Unauthorized(new { message = "Неверный email или пароль." });

        if (!user.IsActive)
            return Unauthorized(new { message = "Аккаунт деактивирован." });

        return Ok(BuildResponse(user));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserProfileDto>> Me()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(idClaim, out var userId))
            return Unauthorized();

        var user = await db.Users.FindAsync(userId);
        if (user is null || !user.IsActive) return Unauthorized();

        return Ok(new UserProfileDto
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            Role = user.Role
        });
    }

    private AuthResponseDto BuildResponse(User user) => new()
    {
        Token = tokenService.GenerateToken(user),
        User = new UserProfileDto
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            Role = user.Role
        }
    };

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLower();
    }

    private static bool VerifyPassword(string password, string hash) =>
        HashPassword(password) == hash;
}
