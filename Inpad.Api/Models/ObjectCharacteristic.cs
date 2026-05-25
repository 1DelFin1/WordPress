namespace Inpad.Api.Models;

public class ObjectCharacteristic
{
    public int Id { get; set; }
    public int ArchObjectId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string? Value { get; set; }
    public string? Unit { get; set; }
    public int SortOrder { get; set; }

    public ArchObject ArchObject { get; set; } = null!;
}
