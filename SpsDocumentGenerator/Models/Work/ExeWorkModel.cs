using System.IO;
using SpsDocumentGenerator.Models.Content;

namespace SpsDocumentGenerator.Models.Work;

public class ExeWorkModel
{
    public ExeWorkModel()
    {
    }

    public ExeWorkModel(ExeFileItem item)
    {
        RelativePath = Path.Combine(item.RelativeDirectoryPath, item.FileName);
        Section = item.Section;
        PartNumber = item.PartNumber;
        Description = item.Description;
    }

    public string RelativePath { get; set; }
    public string Section { get; set; }
    public string PartNumber { get; set; }
    public string Description { get; set; }
}