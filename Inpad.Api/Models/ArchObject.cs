namespace Inpad.Api.Models;

public class ArchObject
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ShortName { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public string? ObjectType { get; set; }
    public ProjectStatus? ProjectStatus { get; set; }
    public DesignStage? DesignStage { get; set; }
    public ObjectStatus Status { get; set; } = ObjectStatus.Draft;
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

    public WordPressStatus WordPressStatus { get; set; } = WordPressStatus.NotPublished;
    public int? WordPressPostId { get; set; }
    public DateTime? PublishedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public int? CreatedByUserId { get; set; }

    public User? CreatedBy { get; set; }
    public List<ObjectMedia> Media { get; set; } = [];
    public List<ObjectCharacteristic> Characteristics { get; set; } = [];
    public List<ObjectTeamMember> TeamMembers { get; set; } = [];
    public List<ObjectCategory> Categories { get; set; } = [];
}
