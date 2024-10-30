using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ClassDocumentGenerator.Lib;
using ClassDocumentGenerator.Lib.DocumentWriter;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace ClassDocumentGenerator
{
    internal class Program
    {
        private static readonly Dictionary<string, List<string>> groups = new();

        private static readonly Dictionary<string, OptionType> options = new()
        {
            ["-public"] = OptionType.Switch,
            ["-findCaller"] = OptionType.Switch,
            ["-word"] = OptionType.Switch,
            ["-ignore"] = OptionType.Value,
            ["-excludePaths"] = OptionType.Value
        };

        private static string solutionPath;

        [STAThread]
        private static int Main(string[] args)
        {
            var parameters = MakeParameters(args);
            if (!parameters.ContainsKey("in"))
            {
                Console.WriteLine("Specify a source file name or directory name.");
                return -1;
            }

            if (!GenerateClassDocumentFromDir(parameters)) return -1;

            Process.GetCurrentProcess().Kill();
            return 0;
        }

        private static bool GenerateClassDocumentFromDir(Dictionary<string, string> parameters)
        {
            if (!SetupInOutRoot(parameters, out var inputRoot, out var outputRoot)) return false;
            if (!SetupSolutionPath(parameters, inputRoot)) return false;

            var excludePaths = SetupExcludePaths(parameters, inputRoot);
            var files = Directory.EnumerateFiles(inputRoot, "*.cs", SearchOption.AllDirectories);
            var error = false;
            List<string> messages = new ();
            foreach (var inputFile in files)
            {
                if (excludePaths
                    .Select(p => CombinePath(inputRoot, p))
                    .Any(p => inputFile.StartsWith(p, StringComparison.InvariantCultureIgnoreCase)))
                {
                    Console.WriteLine($"Skipped \"{inputFile}\"...");
                    continue;
                }

                if (excludePaths.Any(s => inputFile.Contains(s)))
                {
                    Console.WriteLine($"Skipped \"{inputFile}\"...");
                    continue;
                }

                Console.WriteLine($"Processing \"{inputFile}\"...");
                try
                {
                    var outputDir = CombinePath(outputRoot, Path.GetDirectoryName(inputFile.Replace(inputRoot, "")));
                    using var stream = new FileStream(inputFile, FileMode.Open, FileAccess.Read);
                    AddGroup(ParseAndGenerateDocument(parameters, stream, outputDir, inputFile));
                }
                catch (Exception e)
                {
                    Console.WriteLine($"error {inputFile}");
                    error = true;
                    messages.Add($"[{inputFile}] {e.Message}");
                }
            }

            // 통합 integrator
            // using var writer = new HwpDocumentWriter(outputRoot);
            using var writer = DocumentWriterFactory.CreateDocumentWriter(parameters, outputRoot);
            writer.Merge(groups);

            if (error)
            {
                Console.WriteLine("There were files that could not be processed.");
                foreach (var message in messages)
                {
                    Console.WriteLine(message);
                }
                return false;
            }

            return true;
        }

        private static bool SetupSolutionPath(Dictionary<string, string> parameters, string inputRoot)
        {
            var findCaller = parameters.ContainsKey("-findCaller");
            if (findCaller)
            {
                var fileInfo = new DirectoryInfo(inputRoot).GetFiles("*.sln").FirstOrDefault();
                if (fileInfo == null)
                {
                    Console.WriteLine("Find Caller Need to Solution File(*.sln)");
                    Console.WriteLine("Enter key to exit");
                    Console.ReadKey();
                    return false;
                }

                solutionPath = fileInfo.FullName;
            }

            return true;
        }

        private static List<(string, string)> ParseAndGenerateDocument(Dictionary<string, string> parameters, FileStream stream, string outputDir,
            string inputFile)
        {
            var tree = CSharpSyntaxTree.ParseText(SourceText.From(stream));
            var root = tree.GetRoot();
            var ignoreAcc = GetIgnoreAccessibilities(parameters);
            // using var writer = new HwpDocumentWriter(outputDir);
            using var writer = DocumentWriterFactory.CreateDocumentWriter(parameters, outputDir);

            var findCaller = parameters.ContainsKey("-findCaller");
            var gen = new DocumentGenerator(writer, Path.GetFileName(inputFile), solutionPath, findCaller, ignoreAcc);
            return gen.GenerateAndReturnGroupNameAndFileNames(root);
        }

        private static void AddGroup(List<(string, string)> groupNamesAndFilePaths)
        {
            foreach (var groupNamesAndFilePath in groupNamesAndFilePaths)
            {
                var groupNames = groupNamesAndFilePath.Item1;
                var filePath = groupNamesAndFilePath.Item2;

                if (!string.IsNullOrEmpty(groupNames))
                {
                    var groupNameList = groupNames.Split("\r\n");
                    foreach (var groupName in groupNameList)
                    {
                        if (!groups.ContainsKey(groupName)) groups.Add(groupName, new List<string>());
                        groups[groupName].Add(filePath);
                    }
                }
            }
        }

        private static List<string> SetupExcludePaths(Dictionary<string, string> parameters, string inputRoot)
        {
            var excludePaths = new List<string>();
            var gendocExclude = CombinePath(inputRoot, ".gendoc_exclude");
            if (File.Exists(gendocExclude))
                excludePaths = File
                    .ReadAllLines(gendocExclude)
                    .Where(path => !string.IsNullOrWhiteSpace(path))
                    .Select(path => path.Trim())
                    .ToList();

            if (parameters.ContainsKey("-excludePaths"))
            {
                var splitOptions = StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries;
                excludePaths.AddRange(parameters["-excludePaths"].Split(',', splitOptions));
            }

            return excludePaths;
        }

        private static bool SetupInOutRoot(Dictionary<string, string> parameters, out string inputRoot, out string outputRoot)
        {
            inputRoot = parameters["in"];
            outputRoot = string.Empty;

            if (!Directory.Exists(inputRoot))
            {
                Console.WriteLine($"Directory \"{inputRoot}\" does not exist.");
                return false;
            }

            // Use GetFullPath to fully support relative paths.
            outputRoot = Path.GetFullPath(inputRoot);
            if (parameters.ContainsKey("out"))
            {
                outputRoot = parameters["out"];
                try
                {
                    if (Directory.Exists(outputRoot)) Directory.Delete(outputRoot, true);
                    Directory.CreateDirectory(outputRoot);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return false;
                }
            }

            return true;
        }

        private static Dictionary<string, string> MakeParameters(string[] args)
        {
            var currentKey = "";
            var parameters = new Dictionary<string, string>();

            foreach (var arg in args) currentKey = MakeParameter(currentKey, parameters, arg);

            return parameters;
        }

        private static string MakeParameter(string currentKey, Dictionary<string, string> parameters, string arg)
        {
            if (currentKey != string.Empty)
            {
                parameters.Add(currentKey, arg);
                currentKey = "";
                return currentKey;
            }

            if (options.ContainsKey(arg))
            {
                if (options[arg] == OptionType.Value)
                    currentKey = arg;
                else
                    parameters.Add(arg, string.Empty);
            }
            else if (!parameters.ContainsKey("in"))
            {
                parameters.Add("in", arg);
            }
            else if (!parameters.ContainsKey("out"))
            {
                parameters.Add("out", arg);
            }

            return currentKey;
        }

        private static string CombinePath(string first, string second)
        {
            return $"{first.TrimEnd(Path.DirectorySeparatorChar)}{Path.DirectorySeparatorChar}{second.TrimStart(Path.DirectorySeparatorChar)}";
        }

        private static Accessibilities GetIgnoreAccessibilities(Dictionary<string, string> parameters)
        {
            var ignoreAcc = Accessibilities.None;
            if (parameters.ContainsKey("-public"))
            {
                ignoreAcc = Accessibilities.Private |
                            Accessibilities.Internal |
                            Accessibilities.Protected |
                            Accessibilities.ProtectedInternal;
            }
            else if (parameters.ContainsKey("-ignore"))
            {
                var ignoreItems = parameters["-ignore"].Split(',');
                foreach (var item in ignoreItems)
                    if (Enum.TryParse(item, true, out Accessibilities acc))
                        ignoreAcc |= acc;
            }

            return ignoreAcc;
        }

        private enum OptionType
        {
            Value,
            Switch
        }
    }
}