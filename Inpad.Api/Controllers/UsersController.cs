using System.Security.Claims;
using Inpad.Api.Data;
using Inpad.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inpad.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        if (!IsAdmin()) return Forbid();

        var users = await db.Users
            .Select(x => new { x.Id, x.Email, x.Name, x.Role, x.IsActive, x.CreatedAt })
            .ToListAsync();
        return Ok(users);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        if (!IsAdmin()) return Forbid();

        if (await db.Users.AnyAsync(u => u.Email == dto.Email))
            return BadRequest(new { message = "Пользователь с таким email уже существует." });

        if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 6)
            return BadRequest(new { message = "Пароль должен содержать не менее 6 символов." });

        if (!Enum.TryParse<UserRole>(dto.Role, out var role))
            return BadRequest(new { message = "Недопустимая роль." });

        var hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(dto.Password))).ToLower();

        var user = new User
        {
            Email = dto.Email,
            Name = dto.Name,
            PasswordHash = hash,
            Role = role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return Ok(new { user.Id, user.Email, user.Name, user.Role, user.IsActive, user.CreatedAt });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
    {
        if (!IsAdmin()) return Forbid();

        var user = await db.Users.FindAsync(id);
        if (user is null) return NotFound();

        if (dto.Name is not null) user.Name = dto.Name;
        if (dto.IsActive.HasValue) user.IsActive = dto.IsActive.Value;
        if (dto.Role is not null && Enum.TryParse<UserRole>(dto.Role, out var role))
            user.Role = role;

        await db.SaveChangesAsync();

        return Ok(new { user.Id, user.Email, user.Name, user.Role, user.IsActive, user.CreatedAt });
    }

    [HttpPut("{id:int}/role")]
    public async Task<IActionResult> ChangeRole(int id, [FromBody] ChangeRoleDto dto)
    {
        if (!IsAdmin()) return Forbid();

        var user = await db.Users.FindAsync(id);
        if (user is null) return NotFound();

        if (!Enum.TryParse<UserRole>(dto.Role, out var role))
            return BadRequest("Недопустимая роль.");

        user.Role = role;
        await db.SaveChangesAsync();

        return Ok(new { user.Id, user.Email, user.Role });
    }

    [HttpPut("{id:int}/activate")]
    public async Task<IActionResult> Activate(int id)
    {
        if (!IsAdmin()) return Forbid();

        var user = await db.Users.FindAsync(id);
        if (user is null) return NotFound();

        user.IsActive = true;
        await db.SaveChangesAsync();

        return Ok(new { user.Id, user.IsActive });
    }

    [HttpPut("{id:int}/deactivate")]
    public async Task<IActionResult> Deactivate(int id)
    {
        if (!IsAdmin()) return Forbid();

        var user = await db.Users.FindAsync(id);
        if (user is null) return NotFound();

        user.IsActive = false;
        await db.SaveChangesAsync();

        return Ok(new { user.Id, user.IsActive });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!IsAdmin()) return Forbid();

        var currentUserId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid) ? uid : 0;
        if (id == currentUserId) return BadRequest("Нельзя удалить самого себя.");

        var user = await db.Users.FindAsync(id);
        if (user is null) return NotFound();

        db.Users.Remove(user);
        await db.SaveChangesAsync();

        return NoContent();
    }

    private bool IsAdmin() =>
        User.FindFirstValue(ClaimTypes.Role) == "Administrator";
}

public class ChangeRoleDto
{
    public string Role { get; set; } = string.Empty;
}

public class CreateUserDto
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = "Editor";
}

public class UpdateUserDto
{
    public string? Name { get; set; }
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
}
