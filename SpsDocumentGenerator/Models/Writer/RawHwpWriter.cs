using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpsDocumentGenerator.Models.Content;

namespace SpsDocumentGenerator.Models.Writer;

public class RawHwpWriter : HwpWriter
{
    public RawHwpWriter(string path) : base(path)
    {
    }

    protected override string BasePath => Paths.BaseRawHwpFilePath;

    public async Task WriteAsync(List<FileItem> fileItems)
    {
        var relativeDirectoryPathDictionary = GetRelativeDirectoryPathDictionary(fileItems, out var rawFileItems);

        await Task.Run(() => WriteTotalCount(rawFileItems));

        var index = 0;
        foreach (var pair in relativeDirectoryPathDictionary)
        {
            await Task.Run(() => WriteRelativePath(pair.Key));

            foreach (var rawFileItem in pair.Value)
            {
                index++;
                // 진행상황 카운트 및 퍼센트 (소수점 이하 2자리) 표시
                OnProgressChanged(
                    $"원시파일 SPS 문서를 작성하는 중입니다.{Environment.NewLine}{index} / {rawFileItems.Count} [ {Math.Round((double)index / rawFileItems.Count * 100, 2):F2}% ]");
                await Task.Run(() => WriteRowItem(rawFileItem));
            }
        }

        await Task.Run(() => ReplaceText("row", string.Empty));

        await Task.Run(Save);
    }

    private void WriteRowItem(RawFileItem rawFileItem)
    {
        FindText("row");
        Run("TableAppendRow");
        Run("TableColBegin");
        InsertText("row");

        Run("TableUpperCell");
        Run("MoveWordBegin");

        ReplaceText("row", rawFileItem.Index.ToString());
        Run("ParagraphShapeAlignCenter");
        Run("TableRightCell");
        InsertText(rawFileItem.FileName);
        Run("TableRightCell");
        InsertText(rawFileItem.Version);
        Run("TableRightCell");
        InsertText(rawFileItem.DisplaySize);
        Run("TableRightCell");
        InsertText(rawFileItem.Checksum);
        Run("TableRightCell");
        InsertText(rawFileItem.Date.ToString("yyyy.MM.dd"));
        Run("TableRightCell");
        InsertText(rawFileItem.LineCount.ToString());
        Run("TableRightCell");
        InsertText(rawFileItem.Description);
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

    private void WriteTotalCount(ICollection rawFileItems)
    {
        Run("MoveDocBegin");
        // 총 갯수 구하기
        var totalCount = rawFileItems.Count;
        ReplaceText("total_count", totalCount.ToString());
    }

    private Dictionary<string, List<RawFileItem>> GetRelativeDirectoryPathDictionary(IEnumerable<FileItem> fileItems,
        out List<RawFileItem> rawFileItems)
    {
        // RawFileItem으로 캐스팅
        rawFileItems = fileItems.Where(x => x is RawFileItem).Cast<RawFileItem>().ToList();

        // 상대경로 그룹으로 묶기
        var relativeDirectoryPaths = rawFileItems.Select(x => x.RelativeDirectoryPath).Distinct().ToList();

        // 상대경로 그룹별로 파일 아이템 묶기 (딕셔너리)
        var relativeDirectoryPathDictionary = new Dictionary<string, List<RawFileItem>>();
        foreach (var relativeDirectoryPath in relativeDirectoryPaths)
        {
            var items = rawFileItems.Where(x => x.RelativeDirectoryPath == relativeDirectoryPath).ToList();
            relativeDirectoryPathDictionary.Add(relativeDirectoryPath, items);
        }

        return relativeDirectoryPathDictionary;
    }
}