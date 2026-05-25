using System.Security.Claims;
using Inpad.Api.Data;
using Inpad.Api.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inpad.Api.Controllers;

[ApiController]
[Route("api/audit")]
[Authorize]
public class AuditController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] AuditFilterDto filter)
    {
        if (User.FindFirstValue(ClaimTypes.Role) != "Administrator") return Forbid();

        var query = db.AuditLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.EntityType))
            query = query.Where(x => x.EntityType == filter.EntityType);

        if (!string.IsNullOrWhiteSpace(filter.Action))
            query = query.Where(x => x.Action == filter.Action);

        if (filter.UserId.HasValue)
            query = query.Where(x => x.UserId == filter.UserId.Value);

        var total = await query.CountAsync();

        var page = filter.Page < 1 ? 1 : filter.Page;
        var pageSize = filter.PageSize < 1 ? 20 : filter.PageSize;

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new
            {
                x.Id,
                x.Action,
                x.EntityType,
                x.EntityId,
                x.Details,
                x.UserId,
                x.UserEmail,
                x.CreatedAt
            })
            .ToListAsync();

        return Ok(new PagedResultDto<object>
        {
            Items = items.Cast<object>().ToList(),
            Total = total,
            Page = page,
            PageSize = pageSize
        });
    }
}
