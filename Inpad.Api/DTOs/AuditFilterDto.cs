namespace Inpad.Api.DTOs;

public class AuditFilterDto
{
    public string? EntityType { get; set; }
    public string? Action { get; set; }
    public int? UserId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
