using System.IO;
using SpsDocumentGenerator.Models.Content;

namespace SpsDocumentGenerator.Models.Work;

public class RawWorkModel
{
    public RawWorkModel()
    {
    }

    public RawWorkModel(RawFileItem item)
    {
        RelativePath = Path.Combine(item.RelativeDirectoryPath, item.FileName);
        Description = item.Description;
        CsuName = item.CsuName;
    }

    public string RelativePath { get; set; }
    public string Description { get; set; }
    public string CsuName { get; set; }
}