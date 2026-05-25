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
