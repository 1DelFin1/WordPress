using System.Security.Claims;
using Inpad.Api.Data;
using Inpad.Api.Models;
using Inpad.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inpad.Api.Controllers;

[ApiController]
[Route("api/media")]
[Authorize]
public class MediaController(AppDbContext db, IWebHostEnvironment env, AuditService audit) : ControllerBase
{
    private static readonly HashSet<string> AllowedExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg"];

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(
        IFormFile file,
        [FromForm] int archObjectId,
        [FromForm] MediaType mediaType,
        [FromForm] string? title,
        [FromForm] string? altText,
        [FromForm] bool useOnWebsite = false,
        [FromForm] bool useInPresentation = false,
        [FromForm] bool useInPortfolio = false)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            return BadRequest($"Недопустимый тип файла. Разрешены: {string.Join(", ", AllowedExtensions)}");

        var obj = await db.ArchObjects.FindAsync(archObjectId);
        if (obj is null) return NotFound("Объект не найден.");

        var now = DateTime.UtcNow;
        var uploadDir = Path.Combine(env.WebRootPath ?? "wwwroot", "uploads", now.Year.ToString(), now.Month.ToString("D2"));
        Directory.CreateDirectory(uploadDir);

        var fileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadDir, fileName);

        await using (var stream = System.IO.File.Create(filePath))
            await file.CopyToAsync(stream);

        var url = $"/uploads/{now.Year}/{now.Month:D2}/{fileName}";

        var media = new ObjectMedia
        {
            ArchObjectId = archObjectId,
            Url = url,
            FileName = file.FileName,
            Title = title,
            AltText = altText,
            MediaType = mediaType,
            UseOnWebsite = useOnWebsite,
            UseInPresentation = useInPresentation,
            UseInPortfolio = useInPortfolio
        };

        db.ObjectMedias.Add(media);
        await db.SaveChangesAsync();

        var (userId, userEmail) = GetCurrentUser();
        await audit.LogAsync("FileUploaded", "ObjectMedia", media.Id, $"{obj.Name} — {file.FileName}", userId, userEmail);

        return Ok(new { media.Id, media.Url, media.FileName, media.Title, media.MediaType });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMediaDto dto)
    {
        var media = await db.ObjectMedias.FindAsync(id);
        if (media is null) return NotFound();

        if (dto.Title is not null) media.Title = dto.Title;
        if (dto.AltText is not null) media.AltText = dto.AltText;
        if (dto.UseOnWebsite.HasValue) media.UseOnWebsite = dto.UseOnWebsite.Value;
        if (dto.UseInPresentation.HasValue) media.UseInPresentation = dto.UseInPresentation.Value;
        if (dto.UseInPortfolio.HasValue) media.UseInPortfolio = dto.UseInPortfolio.Value;

        await db.SaveChangesAsync();
        return Ok(new { media.Id, media.Title, media.AltText, media.UseOnWebsite, media.UseInPresentation, media.UseInPortfolio });
    }

    [HttpPost("reorder")]
    public async Task<IActionResult> Reorder([FromBody] List<MediaReorderItemDto> items)
    {
        var ids = items.Select(x => x.Id).ToList();
        var mediaList = await db.ObjectMedias.Where(m => ids.Contains(m.Id)).ToListAsync();

        foreach (var item in items)
        {
            var media = mediaList.FirstOrDefault(m => m.Id == item.Id);
            if (media is not null) media.SortOrder = item.SortOrder;
        }

        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var media = await db.ObjectMedias.FindAsync(id);
        if (media is null) return NotFound();

        var physicalPath = Path.Combine(env.WebRootPath ?? "wwwroot", media.Url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (System.IO.File.Exists(physicalPath))
            System.IO.File.Delete(physicalPath);

        db.ObjectMedias.Remove(media);
        await db.SaveChangesAsync();

        return NoContent();
    }

    private (int? userId, string? userEmail) GetCurrentUser()
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userId = int.TryParse(idStr, out var uid) ? uid : (int?)null;
        return (userId, User.FindFirstValue(ClaimTypes.Email));
    }
}

public class UpdateMediaDto
{
    public string? Title { get; set; }
    public string? AltText { get; set; }
    public bool? UseOnWebsite { get; set; }
    public bool? UseInPresentation { get; set; }
    public bool? UseInPortfolio { get; set; }
}

public class MediaReorderItemDto
{
    public int Id { get; set; }
    public int SortOrder { get; set; }
}
