using System.Security.Claims;
using Inpad.Api.Data;
using Inpad.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inpad.Api.Controllers;

[ApiController]
[Route("api/categories")]
[Authorize]
public class CategoriesController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await db.ObjectCategories
            .Where(x => x.IsActive)
            .OrderBy(x => x.SortOrder)
            .Select(x => new { x.Id, x.Name, x.Slug, x.SortOrder })
            .ToListAsync();
        return Ok(categories);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CategoryDto dto)
    {
        if (!IsAdmin()) return Forbid();

        var category = new ObjectCategory
        {
            Name = dto.Name,
            Slug = dto.Slug,
            SortOrder = dto.SortOrder
        };

        db.ObjectCategories.Add(category);
        await db.SaveChangesAsync();

        return Ok(new { category.Id, category.Name, category.Slug, category.SortOrder });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] CategoryDto dto)
    {
        if (!IsAdmin()) return Forbid();

        var category = await db.ObjectCategories.FindAsync(id);
        if (category is null) return NotFound();

        category.Name = dto.Name;
        category.Slug = dto.Slug;
        category.SortOrder = dto.SortOrder;

        await db.SaveChangesAsync();

        return Ok(new { category.Id, category.Name, category.Slug, category.SortOrder });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!IsAdmin()) return Forbid();

        var category = await db.ObjectCategories.FindAsync(id);
        if (category is null) return NotFound();

        category.IsActive = false;
        await db.SaveChangesAsync();

        return NoContent();
    }

    private bool IsAdmin() =>
        User.FindFirstValue(ClaimTypes.Role) == "Administrator";
}

public class CategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public int SortOrder { get; set; }
}
