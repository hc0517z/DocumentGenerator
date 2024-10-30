using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace ClassDocumentGenerator.Lib.DocumentWriter
{
    public class WordDocumentWriter : IDocumentWriter
    {
        private readonly string _path;
        private string _filePath;

        public WordDocumentWriter(string path)
        {
            _path = path;
        }

        private string ProgramDirectory => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public bool ExistsDocumentFile(string fileName)
        {
            var filePath = Path.Combine(_path, $"{fileName}.hwp");
            return File.Exists(filePath);
        }

        public string CreateDocumentFile(string fileName)
        {
            var basePath = Path.Combine(ProgramDirectory, "Base", "BaseDocumentDocx_Style.docx");
            var filePath = Path.Combine(_path, $"{fileName}.docx");
            var directoryName = Path.GetDirectoryName(filePath);
            if (directoryName != null) Directory.CreateDirectory(directoryName);
            if (File.Exists(filePath)) File.Delete(filePath);

            if (File.Exists(filePath)) File.Delete(filePath);
            File.Copy(basePath, filePath);

            _filePath = filePath;
            return filePath;
        }

        public void WriteClassTable(ClassDetails classDetails)
        {
            using var doc = WordprocessingDocument.Open(_filePath, true);
            var body = doc?.MainDocumentPart?.Document?.Body;
            var paras = body?.Elements<Paragraph>().ToList();
            if (paras == null) return;

            paras[0].AppendChild(CreateRunForOpenXml($"{classDetails.ClassName} 클래스"));
            paras[1].AppendChild(CreateRunForOpenXml(classDetails.Description));
            paras[3].AppendChild(CreateRunForOpenXml($" {classDetails.ClassName} 클래스 변수, 함수 목록"));

            var table = body.Elements<Table>().First();

            var varCount = classDetails.ClassTableMembers.Count(member => member.MemberType == ClassTableMemberType.Variable);
            var funcCount = classDetails.ClassTableMembers.Count(member => member.MemberType == ClassTableMemberType.Function);

            var contentRows = table.Elements<TableRow>().Skip(1).ToList();
            var baseVarRow = contentRows.Skip(1).First();
            var baseFuncRow = contentRows.Last();

            if (varCount > 2)
                for (var i = 0; i < varCount - 2; i++)
                    table.InsertBefore(baseVarRow.CloneNode(true), baseVarRow);
            else
                for (var i = 0; i < 2 - varCount; i++)
                    table.RemoveChild(table.Elements<TableRow>().Skip(1).First());

            if (funcCount > 2)
                for (var i = 0; i < funcCount - 2; i++)
                    table.InsertBefore(baseFuncRow.CloneNode(true), baseFuncRow);
            else
                for (var i = 0; i < 2 - funcCount; i++)
                    table.RemoveChild(table.Elements<TableRow>().Skip(1).Last());

            var tableRows = table.Elements<TableRow>().Skip(1).ToList();
            var classTableMembers = classDetails.ClassTableMembers.OrderBy(member => member.MemberType).ToList();

            for (var i = 0; i < classTableMembers.Count; i++)
            {
                var row = tableRows[i];

                row.Elements<TableCell>()
                    .ElementAt(1)
                    .Elements<Paragraph>()
                    .First()
                    .AppendChild(CreateRunForOpenXml(classTableMembers[i].TypeAndReturnType));

                row.Elements<TableCell>()
                    .ElementAt(2)
                    .Elements<Paragraph>()
                    .First()
                    .AppendChild(CreateRunForOpenXml(classTableMembers[i].Name));

                row.Elements<TableCell>()
                    .ElementAt(3)
                    .Elements<Paragraph>()
                    .First()
                    .AppendChild(CreateRunForOpenXml(classTableMembers[i].FunctionDescription));
            }
        }

        public void WriteFunctionDetails(FunctionDetails details)
        {
            using var doc = WordprocessingDocument.Open(_filePath, true);
            var body = doc?.MainDocumentPart?.Document.Body;
            if (body == null) return;

            var bodyLastChild = body?.Last();
            body.InsertBefore(body.ChildElements[5].CloneNode(true), bodyLastChild);
            body.InsertBefore(body.ChildElements[6].CloneNode(true), bodyLastChild);
            body.InsertBefore(body.ChildElements[7].CloneNode(true), bodyLastChild);
            body.InsertBefore(body.ChildElements[8].CloneNode(true), bodyLastChild);
            body.InsertBefore(body.ChildElements[9].CloneNode(true), bodyLastChild);

            body.Elements<Paragraph>()
                .Take(body.Elements<Paragraph>().Count() - 2)
                .Last()
                .AppendChild(CreateRunForOpenXml($"{details.FunctionName}()"));

            body.Elements<Paragraph>()
                .Take(body.Elements<Paragraph>().Count() - 1)
                .Last()
                .AppendChild(CreateRunForOpenXml($" {details.FunctionName}() 함수 내역"));

            var table = body.Elements<Table>().Last();
            var tableRows = table.Elements<TableRow>().ToList();

            tableRows[0]
                .Elements<TableCell>()
                .ElementAt(1)
                .Elements<Paragraph>()
                .First()
                .AppendChild(CreateRunForOpenXml(details.FunctionDescription));

            var parentFunction = details.ParentFunction;
            if (string.IsNullOrEmpty(parentFunction)) parentFunction = details.FunctionName;

            tableRows[1]
                .Elements<TableCell>()
                .ElementAt(1)
                .Elements<Paragraph>()
                .First()
                .AppendChild(CreateRunForOpenXml(parentFunction));

            var isDashOrOneLine = parentFunction.Equals("-") || !parentFunction.Contains("\r\n");
            if (isDashOrOneLine)
                tableRows[1]
                    .Elements<TableCell>()
                    .ElementAt(1)
                    // .Elements<Paragraph>()
                    // .First()
                    .Descendants<Justification>()
                    .First()
                    .Val = JustificationValues.Center;

            tableRows[1]
                .Elements<TableCell>()
                .ElementAt(3)
                .Elements<Paragraph>()
                .First()
                .AppendChild(CreateRunForOpenXml(details.SourceFileName));

            // tableRows[2]
            //     .Elements<TableCell>()
            //     .ElementAt(1)
            //     .Elements<Paragraph>()
            //     .First()
            //     .AppendChild(CreateRunForOpenXml(details.Inputs));
            //
            // tableRows[2]
            //     .Elements<TableCell>()
            //     .ElementAt(3)
            //     .Elements<Paragraph>()
            //     .First()
            //     .AppendChild(CreateRunForOpenXml(details.Output));

            tableRows[4]
                .Elements<TableCell>()
                .ElementAt(0)
                .Elements<Paragraph>()
                .First()
                .AppendChild(CreateRunForOpenXml(details.Process));
        }

        public bool Save()
        {
            using var doc = WordprocessingDocument.Open(_filePath, true);
            var body = doc?.MainDocumentPart?.Document.Body;
            if (body == null) return false;

            var list = body.ChildElements.Skip(5).Take(5).ToList();
            foreach (var element in list) body.RemoveChild(element);
            return true;
        }

        public bool Merge(Dictionary<string, List<string>> groups)
        {
            var dirName = _path;
            if (!Directory.Exists(dirName)) Directory.CreateDirectory(dirName);

            foreach (var (groupsKey, paths) in groups)
            {
                var fileName = System.Text.RegularExpressions.Regex.Replace(groupsKey, @"[\\/:*?""<>|]", "_");
                var groupFileName = Path.Combine(dirName, $"{fileName}.docx");
                Merge(groupFileName, paths);

                Console.WriteLine($"Merged \"{groupFileName}\"...");
            }

            return true;
        }

        public void Dispose()
        {
        }

        public bool Merge(string path, List<string> paths)
        {
            if (paths.Count <= 0) return false;

            var byteArray = File.ReadAllBytes(paths.First());
            using var stream = new MemoryStream();
            stream.Write(byteArray, 0, byteArray.Length);
            using (var myDoc = WordprocessingDocument.Open(stream, true))
            {
                var mainPart = myDoc.MainDocumentPart;
                var body = mainPart?.Document.Body;
                if (body == null) return false;

                body.AppendChild(new Paragraph());
                for (var i = 1; i < paths.Count; i++)
                {
                    var altChunkId = $"AltChunkId{i}";
                    var chunk = mainPart?.AddAlternativeFormatImportPart(AlternativeFormatImportPartType.WordprocessingML, altChunkId);
                    using (var fileStream = File.Open(paths[i], FileMode.Open))
                    {
                        chunk?.FeedData(fileStream);
                    }

                    var altChunk = new AltChunk { Id = altChunkId };
                    //new page, if you like it...
                    body.AppendChild(new Paragraph());
                    //next document
                    body.InsertBefore(altChunk, body.Elements<Paragraph>().Last());
                }

                myDoc.Close();
            }

            File.WriteAllBytes(path, stream.ToArray());

            return true;
        }

        private Run CreateRunForOpenXml(string text)
        {
            var run = new Run();
            string[] newLineArray = { Environment.NewLine };
            var textArray = text.Split(newLineArray, StringSplitOptions.None);

            var first = true;
            foreach (var line in textArray)
            {
                if (!first) run.Append(new Break());
                first = false;
                var txt = new Text { Text = line };
                run.Append(txt);
            }

            return run;
        }
    }
}