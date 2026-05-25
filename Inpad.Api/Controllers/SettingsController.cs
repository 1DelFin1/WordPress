using System.Security.Claims;
using Inpad.Api.Data;
using Inpad.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inpad.Api.Controllers;

[ApiController]
[Route("api/settings")]
[Authorize]
public class SettingsController(AppDbContext db) : ControllerBase
{
    [HttpGet("wordpress")]
    public async Task<IActionResult> GetWordPress()
    {
        if (!IsAdmin()) return Forbid();

        var settings = await db.AppSettings
            .Where(x => x.Key.StartsWith("wp_"))
            .ToDictionaryAsync(x => x.Key, x => x.Value);

        var password = settings.GetValueOrDefault("wp_app_password");
        var masked = string.IsNullOrWhiteSpace(password) ? "" : new string('*', Math.Min(password.Length, 8));

        return Ok(new
        {
            url = settings.GetValueOrDefault("wp_url") ?? "",
            username = settings.GetValueOrDefault("wp_username") ?? "",
            appPassword = masked,
            postType = settings.GetValueOrDefault("wp_post_type") ?? ""
        });
    }

    [HttpPut("wordpress")]
    public async Task<IActionResult> UpdateWordPress([FromBody] WordPressSettingsDto dto)
    {
        if (!IsAdmin()) return Forbid();

        await UpsertSetting("wp_url", dto.Url);
        await UpsertSetting("wp_username", dto.Username);
        if (!string.IsNullOrWhiteSpace(dto.AppPassword))
            await UpsertSetting("wp_app_password", dto.AppPassword);
        await UpsertSetting("wp_post_type", dto.PostType);

        await db.SaveChangesAsync();

        return Ok();
    }

    private async Task UpsertSetting(string key, string? value)
    {
        var setting = await db.AppSettings.FirstOrDefaultAsync(x => x.Key == key);
        if (setting is null)
        {
            db.AppSettings.Add(new AppSetting { Key = key, Value = value });
        }
        else
        {
            setting.Value = value;
        }
    }

    private bool IsAdmin() =>
        User.FindFirstValue(ClaimTypes.Role) == "Administrator";
}

public class WordPressSettingsDto
{
    public string Url { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? AppPassword { get; set; }
    public string PostType { get; set; } = string.Empty;
}
