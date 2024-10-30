using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SpsDocumentGenerator.Models.Checksum;
using SpsDocumentGenerator.Models.Content;

namespace SpsDocumentGenerator.Models.Loader;

public abstract class FileLoader
{
    private readonly string[] _ignoreDir;
    private readonly string[] _ignoreFiles;

    protected readonly ChecksumCreator checksumCreator;

    protected FileLoader(ChecksumCreator checksumCreator, string[] ignoreDir, string[] ignoreFiles)
    {
        this.checksumCreator = checksumCreator;
        _ignoreDir = ignoreDir;
        _ignoreFiles = ignoreFiles;
    }

    public async Task<List<FileItem>> LoadAsync(string path)
    {
        var files = await Task.Run(() => GetFileInfos(path));
        var fileItems = await Task.Run(() => GetFileItems(path, files));
        return fileItems;
    }

    private IEnumerable<FileInfo> GetFileInfos(string path)
    {
        var files = new List<FileInfo>();

        var dirs = Directory.GetDirectories(path, "*", SearchOption.AllDirectories).OrderBy(s => s).ToList();
        dirs.Insert(0, path);

        foreach (var dir in dirs)
        {
            // path에 대한 상대경로 구하기
            var relativeDirectoryPath = GetRelativeDirectoryPath(path, dir);

            // 상대 경로에서 _ignore 디렉토리가 일부라도 포함되어 있으면 continue
            var directorySegments = relativeDirectoryPath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (_ignoreDir.Any(ignoreDir => directorySegments.Contains(ignoreDir))) continue;

            var dirInfo = new DirectoryInfo(dir);

            // _ignoreFile 확장자가 포함되어있으면 FileInfos에서 제외
            var fileInfos = dirInfo.GetFiles().Where(x => !_ignoreFiles.Any(x.Name.Contains)).ToList();

            // fileInfos 확장자 우선순위 (.exe, .dll, .*)로 정렬
            fileInfos = fileInfos.OrderBy(x => x.Extension switch
            {
                ".exe" => 1,
                ".dll" => 2,
                _ => 3
            }).ToList();

            files.AddRange(fileInfos);
        }

        return files;
    }

    private List<FileItem> GetFileItems(string rootPath, IEnumerable<FileInfo> files)
    {
        var fileItems = new List<FileItem>();
        foreach (var (info, i) in files.Select((info, i) => (info, i)))
        {
            var fileItem = CreateFileItem(rootPath, i, info);
            fileItems.Add(fileItem);
        }

        return fileItems;
    }

    protected string GetRelativeDirectoryPath(string rootPath, string directoryName)
    {
        if (string.IsNullOrEmpty(rootPath) || !directoryName.StartsWith(rootPath)) return directoryName;

        var rootPathSpan = rootPath.AsSpan();
        var directoryNameSpan = directoryName.AsSpan();

        var lastIndexOf = rootPathSpan.LastIndexOf('\\');
        if (lastIndexOf == -1) return directoryName;

        return directoryNameSpan.Slice(lastIndexOf + 1).ToString();
    }

    protected abstract FileItem CreateFileItem(string rootPath, int i, FileInfo info);
}