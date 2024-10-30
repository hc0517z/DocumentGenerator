using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using CsvHelper;
using SpsDocumentGenerator.Base;
using SpsDocumentGenerator.Models.Content;
using SpsDocumentGenerator.Models.Export;
using SpsDocumentGenerator.Models.Writer;

namespace SpsDocumentGenerator.Services;

public class ExportService
{
    public Result<bool> ExportCsv(string path, SpsContent content)
    {
        try
        {
            var exeItems = content.ExeFiles.OfType<ExeFileItem>().Select(item => new ExportCsvModel(item));
            var rawItems = content.RawFiles.OfType<RawFileItem>().Select(item => new ExportCsvModel(item));

            using var writer = new StreamWriter(path, false, App.EucKrEncoding);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            // ExportCsvModel 헤더 출력
            csv.WriteHeader<ExportCsvModel>();

            // '실행 파일 목록' 텍스트 출력
            csv.NextRecord();
            csv.WriteField("실행 파일 목록");
            csv.NextRecord();
            csv.WriteRecords(exeItems);

            // '원시 파일 목록' 텍스트 출력
            csv.NextRecord();
            csv.WriteField("원시 파일 목록");
            csv.NextRecord();
            csv.WriteRecords(rawItems);
            return Result<bool>.Success(true);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return Result<bool>.Failure(e.Message);
        }
    }

    public async Task<Result<bool>> ExportSps(SpsContent _content, string _dateString)
    {
        try
        {
            if (!_content.IsEmptyExeFiles)
            {
                var exportExeFileName = Path.Combine(_dateString, $"SPS_EXE_{DateTime.Now.ToString("yyMMdd_HHmmss")}");

                using var exeHwpWriter = Ioc.Default.GetRequiredService<ExeHwpWriter>();
                exeHwpWriter.ProgressChanged += HwpWriterOnProgressChanged;
                await exeHwpWriter.CreateAsync(exportExeFileName);
                await exeHwpWriter.WriteAsync(_content.ExeFiles);
                exeHwpWriter.ProgressChanged -= HwpWriterOnProgressChanged;
            }

            if (!_content.IsEmptyRawFiles)
            {
                var exportRawFileName = Path.Combine(_dateString, $"SPS_RAW_{DateTime.Now.ToString("yyMMdd_HHmmss")}");

                using var rawHwpWriter = Ioc.Default.GetRequiredService<RawHwpWriter>();
                rawHwpWriter.ProgressChanged += HwpWriterOnProgressChanged;
                await rawHwpWriter.CreateAsync(exportRawFileName);
                await rawHwpWriter.WriteAsync(_content.RawFiles);
                rawHwpWriter.ProgressChanged -= HwpWriterOnProgressChanged;
            }

            return Result<bool>.Success(true);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return Result<bool>.Failure(e.Message);
        }
    }

    public event Action<string> ExportSpsProgressChanged;

    private void HwpWriterOnProgressChanged(object sender, string e)
    {
        ExportSpsProgressChanged?.Invoke(e);
    }
}