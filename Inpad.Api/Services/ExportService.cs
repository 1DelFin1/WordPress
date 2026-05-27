using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using Inpad.Api.Models;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System.Text;
using A = DocumentFormat.OpenXml.Drawing;
using P = DocumentFormat.OpenXml.Presentation;
using W = DocumentFormat.OpenXml.Wordprocessing;

namespace Inpad.Api.Services;

public class ExportService
{
    public byte[] ExportDocx(ArchObject obj)
    {
        using var ms = new MemoryStream();
        using var doc = WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document);

        var mainPart = doc.AddMainDocumentPart();
        mainPart.Document = new W.Document();
        var body = mainPart.Document.AppendChild(new W.Body());

        body.AppendChild(Heading(obj.Name, 1));

        var infoTable = new W.Table();
        infoTable.AppendChild(WTableRow("Город", obj.City ?? ""));
        infoTable.AppendChild(WTableRow("Тип", obj.ObjectType ?? ""));
        infoTable.AppendChild(WTableRow("Статус проекта", obj.ProjectStatus?.ToString() ?? ""));
        infoTable.AppendChild(WTableRow("Стадия проектирования", obj.DesignStage?.ToString() ?? ""));
        infoTable.AppendChild(WTableRow("Заказчик", obj.Client ?? ""));
        infoTable.AppendChild(WTableRow("Роль ИНПАД", obj.InpadRole ?? ""));
        infoTable.AppendChild(WTableRow("Годы", FormatYears(obj.YearStart, obj.YearEnd)));
        body.AppendChild(infoTable);

        body.AppendChild(Heading("Описание", 2));
        if (!string.IsNullOrWhiteSpace(obj.ShortDescription))
            body.AppendChild(Para(obj.ShortDescription));
        if (!string.IsNullOrWhiteSpace(obj.FullDescription))
            body.AppendChild(Para(obj.FullDescription));

        if (obj.Characteristics.Count > 0)
        {
            body.AppendChild(Heading("Характеристики", 2));
            var charTable = new W.Table();
            charTable.AppendChild(WTableRow("Показатель", "Значение", "Единица"));
            foreach (var c in obj.Characteristics.OrderBy(x => x.SortOrder))
                charTable.AppendChild(WTableRow(c.Label, c.Value ?? "", c.Unit ?? ""));
            body.AppendChild(charTable);
        }

        if (obj.TeamMembers.Count > 0)
        {
            body.AppendChild(Heading("Команда", 2));
            foreach (var t in obj.TeamMembers.OrderBy(x => x.SortOrder))
                body.AppendChild(Para($"{t.Name} — {t.Role}"));
        }

        mainPart.Document.Save();
        doc.Dispose();
        return ms.ToArray();
    }

    public byte[] ExportPptx(ArchObject obj)
    {
        using var ms = new MemoryStream();
        using var pres = PresentationDocument.Create(ms, PresentationDocumentType.Presentation);

        var presentationPart = pres.AddPresentationPart();
        presentationPart.Presentation = new P.Presentation();

        var slideMasterIdList = new P.SlideMasterIdList();
        var slideIdList = new P.SlideIdList();
        var slideSize = new P.SlideSize { Cx = 9144000, Cy = 5143500 };
        var notesSize = new P.NotesSize { Cx = 6858000, Cy = 9144000 };

        presentationPart.Presentation.AppendChild(slideMasterIdList);
        presentationPart.Presentation.AppendChild(slideIdList);
        presentationPart.Presentation.AppendChild(slideSize);
        presentationPart.Presentation.AppendChild(notesSize);

        var slideMasterPart = presentationPart.AddNewPart<SlideMasterPart>("rId1");
        var slideMaster = new P.SlideMaster(
            new P.CommonSlideData(new P.ShapeTree(
                new P.NonVisualGroupShapeProperties(
                    new P.NonVisualDrawingProperties { Id = 1, Name = "" },
                    new P.NonVisualGroupShapeDrawingProperties(),
                    new P.ApplicationNonVisualDrawingProperties()),
                new P.GroupShapeProperties(new A.TransformGroup()))),
            new P.ColorMap
            {
                Background1 = A.ColorSchemeIndexValues.Light1,
                Text1 = A.ColorSchemeIndexValues.Dark1,
                Background2 = A.ColorSchemeIndexValues.Light2,
                Text2 = A.ColorSchemeIndexValues.Dark2,
                Accent1 = A.ColorSchemeIndexValues.Accent1,
                Accent2 = A.ColorSchemeIndexValues.Accent2,
                Accent3 = A.ColorSchemeIndexValues.Accent3,
                Accent4 = A.ColorSchemeIndexValues.Accent4,
                Accent5 = A.ColorSchemeIndexValues.Accent5,
                Accent6 = A.ColorSchemeIndexValues.Accent6,
                Hyperlink = A.ColorSchemeIndexValues.Hyperlink,
                FollowedHyperlink = A.ColorSchemeIndexValues.FollowedHyperlink
            });
        slideMasterPart.SlideMaster = slideMaster;

        var slideLayoutPart = slideMasterPart.AddNewPart<SlideLayoutPart>("rId1");
        slideLayoutPart.SlideLayout = new P.SlideLayout(
            new P.CommonSlideData(new P.ShapeTree(
                new P.NonVisualGroupShapeProperties(
                    new P.NonVisualDrawingProperties { Id = 1, Name = "" },
                    new P.NonVisualGroupShapeDrawingProperties(),
                    new P.ApplicationNonVisualDrawingProperties()),
                new P.GroupShapeProperties(new A.TransformGroup()))));
        slideLayoutPart.SlideLayout.Save();

        slideMasterPart.AddPart(slideLayoutPart, "rId1");
        slideMasterPart.SlideMaster.Save();

        var slideMasterId = new P.SlideMasterId { Id = 2147483648U, RelationshipId = "rId1" };
        slideMasterIdList.AppendChild(slideMasterId);

        var slidePart = presentationPart.AddNewPart<SlidePart>("rId2");
        var slide = BuildSlide(obj);
        slidePart.Slide = slide;
        slidePart.AddPart(slideLayoutPart, "rId1");
        slide.Save();

        var slideId = new P.SlideId { Id = 256U, RelationshipId = "rId2" };
        slideIdList.AppendChild(slideId);

        presentationPart.Presentation.Save();
        pres.Dispose();
        return ms.ToArray();
    }

    public byte[] ExportTxt(ArchObject obj)
    {
        var sb = new StringBuilder();
        sb.AppendLine(obj.Name);
        if (!string.IsNullOrWhiteSpace(obj.ShortName)) sb.AppendLine($"Краткое название: {obj.ShortName}");
        if (!string.IsNullOrWhiteSpace(obj.City)) sb.AppendLine($"Город: {obj.City}");
        if (!string.IsNullOrWhiteSpace(obj.Address)) sb.AppendLine($"Адрес: {obj.Address}");
        if (!string.IsNullOrWhiteSpace(obj.ObjectType)) sb.AppendLine($"Тип: {obj.ObjectType}");
        if (!string.IsNullOrWhiteSpace(obj.ProjectStatus)) sb.AppendLine($"Статус проекта: {obj.ProjectStatus}");
        if (!string.IsNullOrWhiteSpace(obj.DesignStage)) sb.AppendLine($"Стадия: {obj.DesignStage}");
        sb.AppendLine($"Годы: {FormatYears(obj.YearStart, obj.YearEnd)}");
        if (!string.IsNullOrWhiteSpace(obj.Client)) sb.AppendLine($"Заказчик: {obj.Client}");
        if (!string.IsNullOrWhiteSpace(obj.InpadRole)) sb.AppendLine($"Роль ИНПАД: {obj.InpadRole}");
        sb.AppendLine();

        if (!string.IsNullOrWhiteSpace(obj.ShortDescription))
        {
            sb.AppendLine("ОПИСАНИЕ");
            sb.AppendLine(obj.ShortDescription);
        }
        if (!string.IsNullOrWhiteSpace(obj.FullDescription))
        {
            sb.AppendLine();
            sb.AppendLine(obj.FullDescription);
        }

        if (obj.Characteristics.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("ХАРАКТЕРИСТИКИ");
            foreach (var c in obj.Characteristics.OrderBy(x => x.SortOrder))
                sb.AppendLine($"{c.Label}: {c.Value} {c.Unit}".TrimEnd());
        }

        if (obj.TeamMembers.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("КОМАНДА");
            foreach (var t in obj.TeamMembers.OrderBy(x => x.SortOrder))
                sb.AppendLine($"{t.Name} — {t.Role}");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static W.Paragraph Heading(string text, int level)
    {
        var para = new W.Paragraph();
        var props = new W.ParagraphProperties(new W.ParagraphStyleId { Val = $"Heading{level}" });
        para.AppendChild(props);
        var run = new W.Run(new W.Text(text));
        para.AppendChild(run);
        return para;
    }

    private static W.Paragraph Para(string text)
    {
        var para = new W.Paragraph();
        para.AppendChild(new W.Run(new W.Text(text)));
        return para;
    }

    private static W.TableRow WTableRow(string key, string value)
    {
        var row = new W.TableRow();
        row.AppendChild(WCell(key));
        row.AppendChild(WCell(value));
        return row;
    }

    private static W.TableRow WTableRow(string col1, string col2, string col3)
    {
        var row = new W.TableRow();
        row.AppendChild(WCell(col1));
        row.AppendChild(WCell(col2));
        row.AppendChild(WCell(col3));
        return row;
    }

    private static W.TableCell WCell(string text)
    {
        var cell = new W.TableCell();
        cell.AppendChild(new W.Paragraph(new W.Run(new W.Text(text))));
        return cell;
    }

    private static string FormatYears(int? start, int? end)
    {
        if (start.HasValue && end.HasValue) return $"{start}–{end}";
        if (start.HasValue) return $"{start}–...";
        if (end.HasValue) return $"...–{end}";
        return "";
    }

    public byte[] ExportPdf(ArchObject obj)
    {
        using var ms = new MemoryStream();
        var document = new PdfDocument();
        document.Info.Title = obj.Name;

        var page = document.AddPage();
        page.Size = PdfSharpCore.PageSize.A4;
        var gfx = XGraphics.FromPdfPage(page);

        var fontBold = new XFont("Arial", 18, XFontStyle.Bold);
        var fontBoldMed = new XFont("Arial", 13, XFontStyle.Bold);
        var fontNormal = new XFont("Arial", 11, XFontStyle.Regular);
        var fontSmall = new XFont("Arial", 10, XFontStyle.Regular);
        var fontLabel = new XFont("Arial", 10, XFontStyle.Bold);

        double margin = 50;
        double y = margin;
        double pageWidth = page.Width - margin * 2;

        gfx.DrawString(obj.Name, fontBold, XBrushes.Black, new XRect(margin, y, pageWidth, 30), XStringFormats.TopLeft);
        y += 35;

        void DrawRow(string label, string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            gfx.DrawString(label + ":", fontLabel, XBrushes.Black, new XRect(margin, y, 150, 16), XStringFormats.TopLeft);
            gfx.DrawString(value, fontSmall, XBrushes.Black, new XRect(margin + 155, y, pageWidth - 155, 16), XStringFormats.TopLeft);
            y += 18;
        }

        DrawRow("Город", obj.City);
        DrawRow("Адрес", obj.Address);
        DrawRow("Тип объекта", obj.ObjectType);
        DrawRow("Статус проекта", obj.ProjectStatus?.ToString());
        DrawRow("Стадия", obj.DesignStage?.ToString());
        DrawRow("Годы", FormatYears(obj.YearStart, obj.YearEnd));
        DrawRow("Заказчик", obj.Client);
        DrawRow("Роль ИНПАД", obj.InpadRole);

        y += 10;
        if (!string.IsNullOrWhiteSpace(obj.ShortDescription))
        {
            gfx.DrawString("Описание", fontBoldMed, XBrushes.Black, new XRect(margin, y, pageWidth, 20), XStringFormats.TopLeft);
            y += 22;
            gfx.DrawString(obj.ShortDescription, fontNormal, XBrushes.Black, new XRect(margin, y, pageWidth, 60), XStringFormats.TopLeft);
            y += 65;
        }

        if (obj.Characteristics.Count > 0)
        {
            gfx.DrawString("Характеристики", fontBoldMed, XBrushes.Black, new XRect(margin, y, pageWidth, 20), XStringFormats.TopLeft);
            y += 22;
            foreach (var c in obj.Characteristics.OrderBy(x => x.SortOrder))
            {
                var val = string.IsNullOrWhiteSpace(c.Unit) ? c.Value : $"{c.Value} {c.Unit}";
                DrawRow(c.Label ?? c.Key, val);
            }
        }

        if (obj.TeamMembers.Count > 0)
        {
            y += 5;
            gfx.DrawString("Команда", fontBoldMed, XBrushes.Black, new XRect(margin, y, pageWidth, 20), XStringFormats.TopLeft);
            y += 22;
            foreach (var t in obj.TeamMembers.OrderBy(x => x.SortOrder))
                DrawRow(t.Role ?? "Участник", t.Name);
        }

        document.Save(ms, false);
        return ms.ToArray();
    }

    private static P.Slide BuildSlide(ArchObject obj)
    {
        uint shapeId = 1;

        P.Shape TitleShape(string text, int x, int y, int cx, int cy, int fontSize)
        {
            return new P.Shape(
                new P.NonVisualShapeProperties(
                    new P.NonVisualDrawingProperties { Id = shapeId++, Name = $"Shape{shapeId}" },
                    new P.NonVisualShapeDrawingProperties(new A.ShapeLocks { NoGrouping = true }),
                    new P.ApplicationNonVisualDrawingProperties(new P.PlaceholderShape())),
                new P.ShapeProperties(
                    new A.Transform2D(
                        new A.Offset { X = x, Y = y },
                        new A.Extents { Cx = cx, Cy = cy })),
                new P.TextBody(
                    new A.BodyProperties(),
                    new A.ListStyle(),
                    new A.Paragraph(
                        new A.Run(
                            new A.RunProperties { FontSize = fontSize, Language = "ru-RU" },
                            new A.Text(text)))));
        }

        var slide = new P.Slide(
            new P.CommonSlideData(
                new P.ShapeTree(
                    new P.NonVisualGroupShapeProperties(
                        new P.NonVisualDrawingProperties { Id = shapeId++, Name = "" },
                        new P.NonVisualGroupShapeDrawingProperties(),
                        new P.ApplicationNonVisualDrawingProperties()),
                    new P.GroupShapeProperties(new A.TransformGroup()),
                    TitleShape(obj.Name, 457200, 274638, 8229600, 1143000, 4000),
                    TitleShape($"{obj.City} {FormatYears(obj.YearStart, obj.YearEnd)}".Trim(), 457200, 1600200, 8229600, 600000, 2400),
                    TitleShape(obj.ShortDescription ?? "", 457200, 2400000, 8229600, 1800000, 1800),
                    TitleShape($"{obj.InpadRole} {obj.ObjectType}".Trim(), 457200, 4200000, 8229600, 600000, 1400))),
            new P.ColorMapOverride(new A.MasterColorMapping()));

        return slide;
    }
}
