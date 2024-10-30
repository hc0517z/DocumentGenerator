using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using SpsDocumentGenerator.Models.Content;
using SpsDocumentGenerator.Models.Work;

namespace SpsDocumentGenerator.Services;

public class WorkService
{
    public bool WriteExeWorkCsv(string path, SpsContent content)
    {
        try
        {
            var fileItems = content.ExeFiles.Select(item => new ExeWorkModel(item as ExeFileItem));
            using var writer = new StreamWriter(path, false, App.EucKrEncoding);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csv.WriteRecords(fileItems);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public bool WriteRawWorkCsv(string path, SpsContent content)
    {
        try
        {
            var fileItems = content.RawFiles.Select(item => new RawWorkModel(item as RawFileItem));
            using var writer = new StreamWriter(path, false, App.EucKrEncoding);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csv.WriteRecords(fileItems);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public List<ExeFileItem> ReadExeWorkCsv(string path)
    {
        try
        {
            using var reader = new StreamReader(path, App.EucKrEncoding);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            var records = csv.GetRecords<ExeWorkModel>();
            return records.Select(item => new ExeFileItem
            {
                RelativeDirectoryPath = Path.GetDirectoryName(item.RelativePath)!,
                FileName = Path.GetFileName(item.RelativePath)!,
                Section = item.Section,
                PartNumber = item.PartNumber,
                Description = item.Description
            }).ToList();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new List<ExeFileItem>();
        }
    }

    public List<RawFileItem> ReadRawWorkCsv(string path)
    {
        try
        {
            using var reader = new StreamReader(path, App.EucKrEncoding);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            var records = csv.GetRecords<RawWorkModel>();
            return records.Select(item => new RawFileItem
            {
                RelativeDirectoryPath = Path.GetDirectoryName(item.RelativePath)!,
                FileName = Path.GetFileName(item.RelativePath)!,
                Description = item.Description,
                CsuName = item.CsuName
            }).ToList();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new List<RawFileItem>();
        }
    }

    public bool UpdateExeWork(List<FileItem> target, List<ExeFileItem> updateSource)
    {
        try
        {
            var updated = false;

            foreach (var item in target.OfType<ExeFileItem>())
            {
                var updateItem = updateSource.FirstOrDefault(x =>
                    x.RelativeDirectoryPath == item.RelativeDirectoryPath &&
                    x.FileName == item.FileName);

                if (updateItem != null)
                {
                    item.Section = updateItem.Section;
                    item.PartNumber = updateItem.PartNumber;
                    item.Description = updateItem.Description;

                    updated = true;
                }
            }

            return updated;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public bool UpdateRawWork(List<FileItem> target, List<RawFileItem> updateSource)
    {
        try
        {
            var updated = false;

            foreach (var item in target.OfType<RawFileItem>())
            {
                var updateItem = updateSource.FirstOrDefault(x =>
                    x.RelativeDirectoryPath == item.RelativeDirectoryPath &&
                    x.FileName == item.FileName);

                if (updateItem != null)
                {
                    item.Description = updateItem.Description;
                    item.CsuName = updateItem.CsuName;

                    updated = true;
                }
            }

            return updated;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }
}