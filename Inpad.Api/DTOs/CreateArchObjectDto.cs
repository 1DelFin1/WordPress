using System.ComponentModel.DataAnnotations;
using Inpad.Api.Models;

namespace Inpad.Api.DTOs;

public class CreateArchObjectDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
    public string? ShortName { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public string? ObjectType { get; set; }
    public ProjectStatus? ProjectStatus { get; set; }
    public DesignStage? DesignStage { get; set; }
    public string? ShortDescription { get; set; }
    public List<int> CategoryIds { get; set; } = [];
}
