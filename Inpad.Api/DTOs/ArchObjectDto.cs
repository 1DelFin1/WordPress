using Inpad.Api.Models;

namespace Inpad.Api.DTOs;


public class ObjectMediaDto
{
    public int Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? FileName { get; set; }
    public string? Title { get; set; }
    public string? AltText { get; set; }
    public MediaType MediaType { get; set; }
    public bool UseOnWebsite { get; set; }
    public bool UseInPresentation { get; set; }
    public bool UseInPortfolio { get; set; }
    public int SortOrder { get; set; }
}

public class ObjectCharacteristicDto
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string? Value { get; set; }
    public string? Unit { get; set; }
    public int SortOrder { get; set; }
}

public class ObjectTeamMemberDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Role { get; set; }
    public int SortOrder { get; set; }
}

public class CategoryRefDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class ArchObjectDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ShortName { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public string? ObjectType { get; set; }
    public string? ProjectStatus { get; set; }
    public string? DesignStage { get; set; }
    public ObjectStatus Status { get; set; }
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
    public WordPressStatus WordPressStatus { get; set; }
    public int? WordPressPostId { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int? CreatedByUserId { get; set; }
    public string? MainImageUrl { get; set; }
    public List<ObjectMediaDto> Media { get; set; } = [];
    public List<ObjectCharacteristicDto> Characteristics { get; set; } = [];
    public List<ObjectTeamMemberDto> TeamMembers { get; set; } = [];
    public List<CategoryRefDto> Categories { get; set; } = [];
}
