using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SpsDocumentGenerator.Models;

public class LineCounter
{
    public int CountCodeLines(string filePath)
    {
        var lineCount = 0;
        var task = Task.Run(async () =>
        {
            await foreach (var line in ReadLinesAsync(filePath))
            {
                if (!string.IsNullOrEmpty(line))
                    lineCount++;
            }
        });
        task.Wait(); // 비동기 작업이 완료될 때까지 대기
        return lineCount;
    }
    
    private async IAsyncEnumerable<string> ReadLinesAsync(string filePath)
    {
        using var stream = new StreamReader(filePath);
        while (await stream.ReadLineAsync() is { } line)
        {
            yield return line;
        }
    }
}