using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpsDocumentGenerator.Models.Content;

namespace SpsDocumentGenerator.Models.Writer;

public class ExeHwpWriter : HwpWriter
{
    public ExeHwpWriter(string path) : base(path)
    {
    }

    protected override string BasePath => Paths.BaseExeHwpFilePath;

    public async Task WriteAsync(List<FileItem> fileItems)
    {
        var relativeDirectoryPathDictionary = GetRelativeDirectoryPathDictionary(fileItems, out var exeFileItems);

        await Task.Run(() => WriteTotalCount(exeFileItems));

        var index = 0;
        foreach (var pair in relativeDirectoryPathDictionary)
        {
            await Task.Run(() => WriteRelativePath(pair.Key));

            foreach (var exeFileItem in pair.Value)
            {
                index++;
                // 진행상황 카운트 및 퍼센트 (소수점 이하 2자리) 표시
                OnProgressChanged(
                    $"실행파일 SPS 문서를 작성하는 중입니다.{Environment.NewLine}{index} / {exeFileItems.Count} [ {Math.Round((double)index / exeFileItems.Count * 100, 2):F2}% ]");
                await Task.Run(() => WriteRowItem(exeFileItem));
            }
        }

        await Task.Run(() => ReplaceText("row", string.Empty));

        await Task.Run(Save);
    }

    private void WriteRowItem(ExeFileItem exeFileItem)
    {
        FindText("row");
        Run("TableAppendRow");
        Run("TableColBegin");
        InsertText("row");

        Run("TableUpperCell");
        Run("MoveWordBegin");

        ReplaceText("row", exeFileItem.Section);
        Run("TableRightCell");
        InsertText(exeFileItem.Index.ToString());
        Run("ParagraphShapeAlignCenter");
        Run("TableRightCell");
        InsertText(exeFileItem.FileName);
        Run("TableRightCell");
        InsertText(exeFileItem.Version);
        Run("TableRightCell");
        InsertText(exeFileItem.DisplaySize);
        Run("TableRightCell");
        InsertText(exeFileItem.Checksum);
        Run("TableRightCell");
        InsertText(exeFileItem.Date.ToString("yyyy.MM.dd"));
        Run("TableRightCell");
        InsertText(exeFileItem.PartNumber);
        Run("TableRightCell");
        InsertText(exeFileItem.Description);
    }

    private void WriteRelativePath(string relativePath)
    {
        FindText("row");
        Run("TableAppendRow");
        Run("TableColBegin");
        InsertText("row");

        Run("TableUpperCell");
        Run("MoveWordBegin");

        FindText("row");
        Run("TableCellBlockRow");
        Run("TableMergeCell");

        ReplaceText("row", relativePath);
    }

    private void WriteTotalCount(ICollection exeFileItems)
    {
        Run("MoveDocBegin");
        // 총 갯수 구하기
        var totalCount = exeFileItems.Count;
        ReplaceText("total_count", totalCount.ToString());
    }

    private Dictionary<string, List<ExeFileItem>> GetRelativeDirectoryPathDictionary(IEnumerable<FileItem> fileItems,
        out List<ExeFileItem> exeFileItems)
    {
        // ExeFileItem으로 캐스팅
        exeFileItems = fileItems.Where(x => x is ExeFileItem).Cast<ExeFileItem>().ToList();

        // 상대경로 그룹으로 묶기
        var relativeDirectoryPaths = exeFileItems.Select(x => x.RelativeDirectoryPath).Distinct().ToList();

        // 상대경로 그룹별로 파일 아이템 묶기 (딕셔너리)
        var relativeDirectoryPathDictionary = new Dictionary<string, List<ExeFileItem>>();
        foreach (var relativeDirectoryPath in relativeDirectoryPaths)
        {
            var items = exeFileItems.Where(x => x.RelativeDirectoryPath == relativeDirectoryPath).ToList();
            relativeDirectoryPathDictionary.Add(relativeDirectoryPath, items);
        }

        return relativeDirectoryPathDictionary;
    }
}