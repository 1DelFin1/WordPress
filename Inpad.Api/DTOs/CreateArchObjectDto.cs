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
    public int? YearStart { get; set; }
    public int? YearEnd { get; set; }
    public string? Client { get; set; }
    public string? InpadRole { get; set; }
    public string? ShortDescription { get; set; }
    public string? FullDescription { get; set; }
    public string? SeoTitle { get; set; }
    public string? SeoDescription { get; set; }
    public string? SeoKeywords { get; set; }
    public string? Slug { get; set; }
    public string? OgImageUrl { get; set; }
    public List<int> CategoryIds { get; set; } = [];
    public List<CharacteristicUpsertDto> Characteristics { get; set; } = [];
    public List<TeamMemberUpsertDto> TeamMembers { get; set; } = [];
}
