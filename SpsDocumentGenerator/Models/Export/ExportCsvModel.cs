using SpsDocumentGenerator.Converters;
using SpsDocumentGenerator.Models.Content;

namespace SpsDocumentGenerator.Models.Export;

public class ExportCsvModel
{
    public ExportCsvModel()
    {
    }

    public ExportCsvModel(ExeFileItem item)
    {
        Section = item.Section;
        Index = item.Index.ToString();
        FullPath = item.FullPath;
        DirectoryPath = item.DirectoryPath;
        RelativeDirectoryPath = item.RelativeDirectoryPath;
        FileName = item.FileName;
        Version = item.Version;
        Size = item.DisplaySize;
        Checksum = item.Checksum;
        Date = item.Date.ToString("yyyy.MM.dd");
        PartNumber = item.PartNumber;
        Description = item.Description;
    }

    public ExportCsvModel(RawFileItem item)
    {
        Index = item.Index.ToString();
        FullPath = item.FullPath;
        DirectoryPath = item.DirectoryPath;
        RelativeDirectoryPath = item.RelativeDirectoryPath;
        FileName = item.FileName;
        Version = item.Version;
        Size = item.DisplaySize;
        Checksum = item.Checksum;
        Date = item.Date.ToString("yyyy.MM.dd");
        LineCount = item.LineCount.ToString();
        CsuName = item.CsuName;
        Description = item.Description;
    }

    public string Section { get; set; } = "-";
    public string Index { get; set; }
    public string FullPath { get; set; }
    public string DirectoryPath { get; set; }
    public string RelativeDirectoryPath { get; set; }
    public string FileName { get; set; }
    public string Version { get; set; }
    public string Size { get; set; }
    public string Checksum { get; set; }
    public string Date { get; set; }
    public string PartNumber { get; set; } = "-";
    public string LineCount { get; set; } = "-";
    public string CsuName { get; set; } = "-";
    public string Description { get; set; }
}