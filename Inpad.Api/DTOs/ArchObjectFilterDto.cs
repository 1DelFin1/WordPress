using Inpad.Api.Models;

namespace Inpad.Api.DTOs;

public class ArchObjectFilterDto
{
    public string? SearchQuery { get; set; }
    public ObjectStatus? Status { get; set; }
    public string? City { get; set; }
    public string? ObjectType { get; set; }
    public int? YearStart { get; set; }
    public int? CategoryId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
