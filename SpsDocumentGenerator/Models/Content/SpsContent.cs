using System.Collections.Generic;
using System.Linq;

namespace SpsDocumentGenerator.Models.Content;

public class SpsContent
{
    public bool IsEmpty => IsEmptyExeFiles && IsEmptyRawFiles;

    public bool IsEmptyExeFiles => !ExeFiles.Any();

    public bool IsEmptyRawFiles => !RawFiles.Any();

    public List<FileItem> ExeFiles { get; set; } = new();
    public List<FileItem> RawFiles { get; set; } = new();

    public List<FileItem> AllFiles => new(ExeFiles.Concat(RawFiles));

    public void ApplyIgnoreFiles(string[] ignoreFiles)
    {
        ExeFiles = ExeFiles.Where(x => !ignoreFiles.Any(x.FileName.Contains)).ToList();
        RawFiles = RawFiles.Where(x => !ignoreFiles.Any(x.FileName.Contains)).ToList();
    }

    public void ApplyIgnoreDirectories(string[] ignoreDirectories)
    {
        ExeFiles = ExeFiles.Where(x => !ignoreDirectories.Any(x.RelativeDirectoryPath.Contains)).ToList();
        RawFiles = RawFiles.Where(x => !ignoreDirectories.Any(x.RelativeDirectoryPath.Contains)).ToList();
    }
}