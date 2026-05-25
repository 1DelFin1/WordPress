namespace Inpad.Api.Models;

public class ObjectTeamMember
{
    public int Id { get; set; }
    public int ArchObjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Role { get; set; }
    public int SortOrder { get; set; }

    public ArchObject ArchObject { get; set; } = null!;
}
