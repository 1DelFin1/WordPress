using System.Security.Claims;
using Inpad.Api.Data;
using Inpad.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inpad.Api.Controllers;

[ApiController]
[Route("api/references")]
[Authorize]
public class ReferencesController(AppDbContext db) : ControllerBase
{
    // GET /api/references/cities
    // GET /api/references/object-types
    // GET /api/references/inpad-roles
    [HttpGet("{type}")]
    public async Task<ActionResult<List<ReferenceItemDto>>> GetByType(string type)
    {
        var key = NormalizeType(type);
        var items = await db.References
            .Where(x => x.Type == key && x.IsActive)
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Value)
            .Select(x => new ReferenceItemDto { Id = x.Id, Value = x.Value, SortOrder = x.SortOrder })
            .ToListAsync();
        return Ok(items);
    }

    [HttpPost("{type}")]
    public async Task<ActionResult<ReferenceItemDto>> Create(string type, [FromBody] ReferenceUpsertDto dto)
    {
        if (!IsAdmin()) return Forbid();
        var key = NormalizeType(type);
        var item = new Reference { Type = key, Value = dto.Value.Trim(), SortOrder = dto.SortOrder };
        db.References.Add(item);
        await db.SaveChangesAsync();
        return Ok(new ReferenceItemDto { Id = item.Id, Value = item.Value, SortOrder = item.SortOrder });
    }

    [HttpPut("{type}/{id:int}")]
    public async Task<ActionResult<ReferenceItemDto>> Update(string type, int id, [FromBody] ReferenceUpsertDto dto)
    {
        if (!IsAdmin()) return Forbid();
        var key = NormalizeType(type);
        var item = await db.References.FirstOrDefaultAsync(x => x.Id == id && x.Type == key);
        if (item is null) return NotFound();
        item.Value = dto.Value.Trim();
        item.SortOrder = dto.SortOrder;
        await db.SaveChangesAsync();
        return Ok(new ReferenceItemDto { Id = item.Id, Value = item.Value, SortOrder = item.SortOrder });
    }

    [HttpDelete("{type}/{id:int}")]
    public async Task<IActionResult> Delete(string type, int id)
    {
        if (!IsAdmin()) return Forbid();
        var key = NormalizeType(type);
        var item = await db.References.FirstOrDefaultAsync(x => x.Id == id && x.Type == key);
        if (item is null) return NotFound();
        item.IsActive = false;
        await db.SaveChangesAsync();
        return NoContent();
    }

    private static string NormalizeType(string type) => type.ToLowerInvariant().Replace("-", "_");

    private bool IsAdmin() =>
        User.FindFirstValue(ClaimTypes.Role) == nameof(UserRole.Administrator);
}

public class ReferenceItemDto
{
    public int Id { get; set; }
    public string Value { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

public class ReferenceUpsertDto
{
    public string Value { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}
