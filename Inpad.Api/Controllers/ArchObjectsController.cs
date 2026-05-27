using System.Security.Claims;
using Inpad.Api.Data;
using Inpad.Api.DTOs;
using Inpad.Api.Models;
using Inpad.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inpad.Api.Controllers;

[ApiController]
[Route("api/objects")]
[Authorize]
public class ArchObjectsController(AppDbContext db, WordPressService wp, AuditService audit, ExportService export) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResultDto<ArchObjectListItemDto>>> GetList([FromQuery] ArchObjectFilterDto filter)
    {
        var query = db.ArchObjects.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchQuery))
        {
            var q = filter.SearchQuery.ToLower();
            query = query.Where(x =>
                x.Name.ToLower().Contains(q) ||
                (x.City != null && x.City.ToLower().Contains(q)));
        }

        if (filter.Status.HasValue)
            query = query.Where(x => x.Status == filter.Status.Value);

        if (!string.IsNullOrWhiteSpace(filter.City))
            query = query.Where(x => x.City != null && x.City.ToLower().Contains(filter.City.ToLower()));

        if (!string.IsNullOrWhiteSpace(filter.ObjectType))
            query = query.Where(x => x.ObjectType != null && x.ObjectType.ToLower().Contains(filter.ObjectType.ToLower()));

        if (filter.YearStart.HasValue)
            query = query.Where(x => x.YearStart == filter.YearStart || x.YearEnd == filter.YearStart);

        if (filter.CategoryId.HasValue)
            query = query.Where(x => x.Categories.Any(c => c.Id == filter.CategoryId));

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(x => x.UpdatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(x => new ArchObjectListItemDto
            {
                Id = x.Id,
                Name = x.Name,
                ShortName = x.ShortName,
                City = x.City,
                ObjectType = x.ObjectType,
                ProjectStatus = x.ProjectStatus,
                DesignStage = x.DesignStage,
                Status = x.Status,
                WordPressStatus = x.WordPressStatus,
                MainImageUrl = x.Media
                    .Where(m => m.MediaType == MediaType.MainImage)
                    .OrderBy(m => m.SortOrder)
                    .Select(m => m.Url)
                    .FirstOrDefault(),
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync();

        return Ok(new PagedResultDto<ArchObjectListItemDto>
        {
            Items = items,
            Total = total,
            Page = filter.Page,
            PageSize = filter.PageSize
        });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ArchObjectDto>> GetById(int id)
    {
        var obj = await db.ArchObjects
            .Include(x => x.Media)
            .Include(x => x.Characteristics)
            .Include(x => x.TeamMembers)
            .Include(x => x.Categories)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (obj is null) return NotFound();

        return Ok(MapToDto(obj));
    }

    [HttpPost]
    public async Task<ActionResult<ArchObjectDto>> Create([FromBody] CreateArchObjectDto dto)
    {
        var obj = new ArchObject
        {
            Name = dto.Name,
            ShortName = dto.ShortName,
            City = dto.City,
            Address = dto.Address,
            ObjectType = dto.ObjectType,
            ProjectStatus = dto.ProjectStatus,
            DesignStage = dto.DesignStage,
            ShortDescription = dto.ShortDescription
        };

        if (dto.CategoryIds.Count > 0)
        {
            var categories = await db.ObjectCategories
                .Where(x => dto.CategoryIds.Contains(x.Id))
                .ToListAsync();
            obj.Categories = categories;
        }

        db.ArchObjects.Add(obj);
        await db.SaveChangesAsync();

        var (userId, userEmail) = GetCurrentUser();
        await audit.LogAsync("ObjectCreated", "ArchObject", obj.Id, obj.Name, userId, userEmail);

        return CreatedAtAction(nameof(GetById), new { id = obj.Id }, MapToDto(obj));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ArchObjectDto>> Update(int id, [FromBody] UpdateArchObjectDto dto)
    {
        var obj = await db.ArchObjects
            .Include(x => x.Media)
            .Include(x => x.Characteristics)
            .Include(x => x.TeamMembers)
            .Include(x => x.Categories)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (obj is null) return NotFound();

        if (dto.Name is not null) obj.Name = dto.Name;
        if (dto.ShortName is not null) obj.ShortName = dto.ShortName;
        if (dto.City is not null) obj.City = dto.City;
        if (dto.Address is not null) obj.Address = dto.Address;
        if (dto.ObjectType is not null) obj.ObjectType = dto.ObjectType;
        if (dto.ProjectStatus is not null) obj.ProjectStatus = dto.ProjectStatus;
        if (dto.DesignStage is not null) obj.DesignStage = dto.DesignStage;
        if (dto.YearStart.HasValue) obj.YearStart = dto.YearStart;
        if (dto.YearEnd.HasValue) obj.YearEnd = dto.YearEnd;
        if (dto.Client is not null) obj.Client = dto.Client;
        if (dto.InpadRole is not null) obj.InpadRole = dto.InpadRole;
        if (dto.ShortDescription is not null) obj.ShortDescription = dto.ShortDescription;
        if (dto.FullDescription is not null) obj.FullDescription = dto.FullDescription;
        if (dto.SeoTitle is not null) obj.SeoTitle = dto.SeoTitle;
        if (dto.SeoDescription is not null) obj.SeoDescription = dto.SeoDescription;
        if (dto.SeoKeywords is not null) obj.SeoKeywords = dto.SeoKeywords;
        if (dto.Slug is not null) obj.Slug = dto.Slug;
        if (dto.OgImageUrl is not null) obj.OgImageUrl = dto.OgImageUrl;

        if (dto.CategoryIds is not null)
        {
            var categories = await db.ObjectCategories
                .Where(x => dto.CategoryIds.Contains(x.Id))
                .ToListAsync();
            obj.Categories = categories;
        }

        if (dto.Characteristics is not null)
        {
            obj.Characteristics.Clear();
            obj.Characteristics = dto.Characteristics.Select(c => new ObjectCharacteristic
            {
                Key = c.Key,
                Label = c.Label,
                Value = c.Value,
                Unit = c.Unit,
                SortOrder = c.SortOrder
            }).ToList();
        }

        if (dto.TeamMembers is not null)
        {
            obj.TeamMembers.Clear();
            obj.TeamMembers = dto.TeamMembers.Select(t => new ObjectTeamMember
            {
                Name = t.Name,
                Role = t.Role,
                SortOrder = t.SortOrder
            }).ToList();
        }

        obj.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        var (userId, userEmail) = GetCurrentUser();
        await audit.LogAsync("ObjectEdited", "ArchObject", obj.Id, obj.Name, userId, userEmail);

        return Ok(MapToDto(obj));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var obj = await db.ArchObjects.FindAsync(id);
        if (obj is null) return NotFound();

        var (userId, userEmail) = GetCurrentUser();

        db.ArchObjects.Remove(obj);
        await db.SaveChangesAsync();

        await audit.LogAsync("ObjectDeleted", "ArchObject", id, obj.Name, userId, userEmail);

        return NoContent();
    }

    [HttpPost("{id:int}/submit")]
    public async Task<ActionResult<ArchObjectDto>> Submit(int id)
    {
        var obj = await db.ArchObjects
            .Include(x => x.Media)
            .Include(x => x.Characteristics)
            .Include(x => x.TeamMembers)
            .Include(x => x.Categories)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (obj is null) return NotFound();

        obj.Status = ObjectStatus.UnderReview;
        obj.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        var (userId, userEmail) = GetCurrentUser();
        await audit.LogAsync("StatusChanged", "ArchObject", obj.Id, $"{obj.Name} → UnderReview", userId, userEmail);

        return Ok(MapToDto(obj));
    }

    [HttpPost("{id:int}/publish")]
    public async Task<ActionResult<ArchObjectDto>> Publish(int id)
    {
        var obj = await db.ArchObjects
            .Include(x => x.Media)
            .Include(x => x.Characteristics)
            .Include(x => x.TeamMembers)
            .Include(x => x.Categories)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (obj is null) return NotFound();

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(obj.Name))
            errors.Add("Название обязательно для публикации.");

        if (string.IsNullOrWhiteSpace(obj.City))
            errors.Add("Город обязателен для публикации.");

        if (string.IsNullOrWhiteSpace(obj.ShortDescription))
            errors.Add("Краткое описание обязательно для публикации.");

        if (string.IsNullOrWhiteSpace(obj.ObjectType))
            errors.Add("Тип объекта обязателен для публикации.");

        if (string.IsNullOrWhiteSpace(obj.InpadRole))
            errors.Add("Роль ИНПАД обязательна для публикации.");

        if (obj.ProjectStatus is null)
            errors.Add("Статус проекта обязателен для публикации.");

        if (!obj.Categories.Any())
            errors.Add("Необходимо выбрать хотя бы одну категорию.");

        if (!obj.Media.Any(m => m.MediaType == MediaType.MainImage))
            errors.Add("Необходимо загрузить главное изображение.");

        var duplicate = await db.ArchObjects
            .Where(x => x.Id != id && x.Status == ObjectStatus.Published &&
                        x.Name.ToLower() == obj.Name.ToLower())
            .AnyAsync();
        if (duplicate)
            errors.Add($"Уже существует опубликованный объект с названием «{obj.Name}».");

        if (errors.Count > 0)
            return BadRequest(new { errors });

        if (wp.IsConfigured)
        {
            try
            {
                var wpPostId = await wp.PublishAsync(obj);
                obj.WordPressPostId = wpPostId;
                obj.WordPressStatus = WordPressStatus.Published;
            }
            catch (Exception ex)
            {
                obj.WordPressStatus = WordPressStatus.PublishError;
                obj.Status = ObjectStatus.PublishError;
                obj.UpdatedAt = DateTime.UtcNow;
                await db.SaveChangesAsync();
                return StatusCode(502, new { message = "Ошибка публикации в WordPress.", detail = ex.Message });
            }
        }

        obj.Status = ObjectStatus.Published;
        obj.PublishedAt = DateTime.UtcNow;
        obj.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        var (userId, userEmail) = GetCurrentUser();
        await audit.LogAsync("ObjectPublished", "ArchObject", obj.Id, obj.Name, userId, userEmail);

        return Ok(MapToDto(obj));
    }

    [HttpPost("{id:int}/unpublish")]
    public async Task<ActionResult<ArchObjectDto>> Unpublish(int id)
    {
        var obj = await db.ArchObjects
            .Include(x => x.Media)
            .Include(x => x.Characteristics)
            .Include(x => x.TeamMembers)
            .Include(x => x.Categories)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (obj is null) return NotFound();

        if (wp.IsConfigured && obj.WordPressPostId.HasValue)
        {
            try
            {
                await wp.UnpublishAsync(obj.WordPressPostId.Value);
                obj.WordPressStatus = WordPressStatus.Unpublished;
            }
            catch (Exception ex)
            {
                return StatusCode(502, new { message = "Ошибка снятия с публикации в WordPress.", detail = ex.Message });
            }
        }

        obj.Status = ObjectStatus.Archived;
        obj.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        var (userId, userEmail) = GetCurrentUser();
        await audit.LogAsync("ObjectUnpublished", "ArchObject", obj.Id, obj.Name, userId, userEmail);

        return Ok(MapToDto(obj));
    }

    [HttpPost("{id:int}/duplicate")]
    public async Task<ActionResult<ArchObjectDto>> Duplicate(int id)
    {
        var obj = await db.ArchObjects
            .Include(x => x.Characteristics)
            .Include(x => x.TeamMembers)
            .Include(x => x.Categories)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (obj is null) return NotFound();

        var copy = new ArchObject
        {
            Name = obj.Name + " (копия)",
            ShortName = obj.ShortName,
            City = obj.City,
            Address = obj.Address,
            ObjectType = obj.ObjectType,
            ProjectStatus = obj.ProjectStatus,
            DesignStage = obj.DesignStage,
            Status = ObjectStatus.Draft,
            YearStart = obj.YearStart,
            YearEnd = obj.YearEnd,
            Client = obj.Client,
            InpadRole = obj.InpadRole,
            ShortDescription = obj.ShortDescription,
            FullDescription = obj.FullDescription,
            SeoTitle = obj.SeoTitle,
            SeoDescription = obj.SeoDescription,
            Characteristics = obj.Characteristics.Select(c => new ObjectCharacteristic
            {
                Key = c.Key,
                Label = c.Label,
                Value = c.Value,
                Unit = c.Unit,
                SortOrder = c.SortOrder
            }).ToList(),
            TeamMembers = obj.TeamMembers.Select(t => new ObjectTeamMember
            {
                Name = t.Name,
                Role = t.Role,
                SortOrder = t.SortOrder
            }).ToList(),
            Categories = obj.Categories.ToList()
        };

        db.ArchObjects.Add(copy);
        await db.SaveChangesAsync();

        var result = await db.ArchObjects
            .Include(x => x.Media)
            .Include(x => x.Characteristics)
            .Include(x => x.TeamMembers)
            .Include(x => x.Categories)
            .FirstAsync(x => x.Id == copy.Id);

        return CreatedAtAction(nameof(GetById), new { id = copy.Id }, MapToDto(result));
    }

    [HttpGet("{id:int}/export/docx")]
    public async Task<IActionResult> ExportDocx(int id)
    {
        var obj = await LoadForExport(id);
        if (obj is null) return NotFound();
        var (userId, userEmail) = GetCurrentUser();
        await audit.LogAsync("ExportGenerated", "ArchObject", id, $"{obj.Name} → DOCX", userId, userEmail);
        var bytes = export.ExportDocx(obj);
        return File(bytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", $"{obj.Name}.docx");
    }

    [HttpGet("{id:int}/export/pptx")]
    public async Task<IActionResult> ExportPptx(int id)
    {
        var obj = await LoadForExport(id);
        if (obj is null) return NotFound();
        var (userId, userEmail) = GetCurrentUser();
        await audit.LogAsync("ExportGenerated", "ArchObject", id, $"{obj.Name} → PPTX", userId, userEmail);
        var bytes = export.ExportPptx(obj);
        return File(bytes, "application/vnd.openxmlformats-officedocument.presentationml.presentation", $"{obj.Name}.pptx");
    }

    [HttpGet("{id:int}/export/txt")]
    public async Task<IActionResult> ExportTxt(int id)
    {
        var obj = await LoadForExport(id);
        if (obj is null) return NotFound();
        var (userId, userEmail) = GetCurrentUser();
        await audit.LogAsync("ExportGenerated", "ArchObject", id, $"{obj.Name} → TXT", userId, userEmail);
        var bytes = export.ExportTxt(obj);
        return File(bytes, "text/plain; charset=utf-8", $"{obj.Name}.txt");
    }

    [HttpGet("{id:int}/export/pdf")]
    public async Task<IActionResult> ExportPdf(int id)
    {
        var obj = await LoadForExport(id);
        if (obj is null) return NotFound();
        var (userId, userEmail) = GetCurrentUser();
        await audit.LogAsync("ExportGenerated", "ArchObject", id, $"{obj.Name} → PDF", userId, userEmail);
        var bytes = export.ExportPdf(obj);
        return File(bytes, "application/pdf", $"{obj.Name}.pdf");
    }

    private Task<ArchObject?> LoadForExport(int id) =>
        db.ArchObjects
            .Include(x => x.Media)
            .Include(x => x.Characteristics)
            .Include(x => x.TeamMembers)
            .Include(x => x.Categories)
            .FirstOrDefaultAsync(x => x.Id == id);

    private (int? userId, string? userEmail) GetCurrentUser()
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userId = int.TryParse(idStr, out var uid) ? uid : (int?)null;
        var userEmail = User.FindFirstValue(ClaimTypes.Email);
        return (userId, userEmail);
    }

    private static ArchObjectDto MapToDto(ArchObject obj) => new()
    {
        Id = obj.Id,
        Name = obj.Name,
        ShortName = obj.ShortName,
        City = obj.City,
        Address = obj.Address,
        ObjectType = obj.ObjectType,
        ProjectStatus = obj.ProjectStatus,
        DesignStage = obj.DesignStage,
        Status = obj.Status,
        YearStart = obj.YearStart,
        YearEnd = obj.YearEnd,
        Client = obj.Client,
        InpadRole = obj.InpadRole,
        ShortDescription = obj.ShortDescription,
        FullDescription = obj.FullDescription,
        SeoTitle = obj.SeoTitle,
        SeoDescription = obj.SeoDescription,
        SeoKeywords = obj.SeoKeywords,
        Slug = obj.Slug,
        OgImageUrl = obj.OgImageUrl,
        WordPressStatus = obj.WordPressStatus,
        WordPressPostId = obj.WordPressPostId,
        PublishedAt = obj.PublishedAt,
        CreatedAt = obj.CreatedAt,
        UpdatedAt = obj.UpdatedAt,
        CreatedByUserId = obj.CreatedByUserId,
        MainImageUrl = obj.Media
            .Where(m => m.MediaType == MediaType.MainImage)
            .OrderBy(m => m.SortOrder)
            .Select(m => m.Url)
            .FirstOrDefault(),
        Media = obj.Media.Select(m => new ObjectMediaDto
        {
            Id = m.Id,
            Url = m.Url,
            FileName = m.FileName,
            Title = m.Title,
            AltText = m.AltText,
            MediaType = m.MediaType,
            UseOnWebsite = m.UseOnWebsite,
            UseInPresentation = m.UseInPresentation,
            UseInPortfolio = m.UseInPortfolio,
            SortOrder = m.SortOrder
        }).ToList(),
        Characteristics = obj.Characteristics.Select(c => new ObjectCharacteristicDto
        {
            Id = c.Id,
            Key = c.Key,
            Label = c.Label,
            Value = c.Value,
            Unit = c.Unit,
            SortOrder = c.SortOrder
        }).ToList(),
        TeamMembers = obj.TeamMembers.Select(t => new ObjectTeamMemberDto
        {
            Id = t.Id,
            Name = t.Name,
            Role = t.Role,
            SortOrder = t.SortOrder
        }).ToList(),
        Categories = obj.Categories.Select(c => new CategoryRefDto
        {
            Id = c.Id,
            Name = c.Name
        }).ToList()
    };
}
