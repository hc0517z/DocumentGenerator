using System.IO;
using SpsDocumentGenerator.Models.Checksum;
using SpsDocumentGenerator.Models.Content;

namespace SpsDocumentGenerator.Models.Loader;

public class ExeFileLoader : FileLoader
{
    public ExeFileLoader(ChecksumCreator checksumCreator, string[] ignoreDir, string[] ignoreFiles) : base(checksumCreator, ignoreDir, ignoreFiles)
    {
    }

    protected override FileItem CreateFileItem(string rootPath, int i, FileInfo info)
    {
        var fileItem = new ExeFileItem
        {
            Index = i + 1,
            FileName = info.Name,
            FullPath = info.FullName,
            DirectoryPath = info.DirectoryName!,
            RelativeDirectoryPath = GetRelativeDirectoryPath(rootPath, info.DirectoryName!),
            Size = info.Length,
            Date = info.LastWriteTime,
            Checksum = checksumCreator.Create(info.FullName),
            Section = GetSection(info.Name)
        };
        return fileItem;
    }

    private string GetSection(string fileName)
    {
        // FileName에서 확장자 추출
        var extension = Path.GetExtension(fileName);

        return extension switch
        {
            ".exe" => "실행파일",
            ".dll" => "DLL 파일",
            _ => "환경파일"
        };
    }
}