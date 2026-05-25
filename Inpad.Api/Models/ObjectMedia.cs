namespace Inpad.Api.Models;

public class ObjectMedia
{
    public int Id { get; set; }
    public int ArchObjectId { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? FileName { get; set; }
    public string? Title { get; set; }
    public string? AltText { get; set; }
    public MediaType MediaType { get; set; }
    public bool UseOnWebsite { get; set; }
    public bool UseInPresentation { get; set; }
    public bool UseInPortfolio { get; set; }
    public int SortOrder { get; set; }

    public ArchObject ArchObject { get; set; } = null!;
}
