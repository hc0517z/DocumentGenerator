using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SpsDocumentGenerator.Models.Checksum;
using SpsDocumentGenerator.Models.Content;

namespace SpsDocumentGenerator.Services;

public class RenewService
{
    public async Task RenewChecksumAsync(HashKind hashKind, SpsContent spsContent)
    {
        var checksumCreator = ChecksumCreatorFactory.Create(hashKind);

        foreach (var file in spsContent.AllFiles)
        {
            var checksum = await Task.Run(() => checksumCreator.Create(file.FullPath));
            file.Checksum = checksum;
        }
    }

    public void RenewIndex(IndexKind indexKind, List<FileItem> list)
    {
        switch (indexKind)
        {
            case IndexKind.All:
                RenewIndexWithAll(list);
                break;
            case IndexKind.Directory:
                RenewIndexWithDirectory(list);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(indexKind), indexKind, null);
        }
    }

    public void RenewIndex(IndexKind indexKind, ref SpsContent spsContent)
    {
        switch (indexKind)
        {
            case IndexKind.All:
                RenewIndexWithAll(spsContent.ExeFiles);
                RenewIndexWithAll(spsContent.RawFiles);
                break;
            case IndexKind.Directory:
                RenewIndexWithDirectory(spsContent.AllFiles);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(indexKind), indexKind, null);
        }
    }

    private void RenewIndexWithAll(List<FileItem> list)
    {
        foreach (var (fileItem, i) in list.Select((fileItem, i) => (fileItem, i))) fileItem.Index = i + 1;
    }

    private void RenewIndexWithDirectory(List<FileItem> list)
    {
        var fileGroups = list.GroupBy(x => x.RelativeDirectoryPath).ToList();

        foreach (var fileGroup in fileGroups)
        foreach (var (fileItem, i) in fileGroup.Select((fileItem, i) => (fileItem, i)))
            fileItem.Index = i + 1;
    }

    public void UpdatePartNumber(string prefix, string indexFormat, List<FileItem> fileItems)
    {
        // ExeFileItem으로 캐스팅
        var exeFileItems = fileItems.Cast<ExeFileItem>().ToList();

        // 확장자 그룹화
        // exe, dll, 그 외
        var extensionGroups = exeFileItems.GroupBy(x =>
        {
            var extension = Path.GetExtension(x.FileName);
            if (extension is ".exe" or ".dll") return extension;
            return "C";
        }).ToList();

        foreach (var extensionGroup in extensionGroups)
        {
            var partNumber = 1;
            foreach (var exeFileItem in extensionGroup)
            {
                exeFileItem.PartNumber = $"{prefix}{extensionGroup.Key.ToUpper()[^1]}{partNumber.ToString(indexFormat)}";
                partNumber++;
            }
        }
    }

    public void ReplaceRootPath(List<FileItem> fileItems, string replaceRootPath)
    {
        if (string.IsNullOrEmpty(replaceRootPath)) return;
        
        foreach (var fileItem in fileItems)
        {
            var rootDirectory = fileItem.RelativeDirectoryPath.Split('\\')[0];
            fileItem.RelativeDirectoryPath = fileItem.RelativeDirectoryPath.Replace(rootDirectory, replaceRootPath);
        }
    }
}