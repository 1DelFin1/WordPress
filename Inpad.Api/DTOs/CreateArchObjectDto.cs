using System.ComponentModel.DataAnnotations;

namespace Inpad.Api.DTOs;

public class CreateArchObjectDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
    public string? ShortName { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public string? ObjectType { get; set; }
    public string? ProjectStatus { get; set; }
    public string? DesignStage { get; set; }
    public string? ShortDescription { get; set; }
    public List<int> CategoryIds { get; set; } = [];
}
