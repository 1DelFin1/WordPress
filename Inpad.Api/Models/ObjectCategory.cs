namespace Inpad.Api.Models;

public class ObjectCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    public List<ArchObject> Objects { get; set; } = [];
}
