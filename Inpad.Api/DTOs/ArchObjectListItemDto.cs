using Inpad.Api.Models;

namespace Inpad.Api.DTOs;

public class ArchObjectListItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ShortName { get; set; }
    public string? City { get; set; }
    public string? ObjectType { get; set; }
    public string? ProjectStatus { get; set; }
    public string? DesignStage { get; set; }
    public ObjectStatus Status { get; set; }
    public WordPressStatus WordPressStatus { get; set; }
    public string? MainImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
