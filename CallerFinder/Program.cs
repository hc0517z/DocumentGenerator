using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CallerFinder
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var solutionPath = args[0];
            var fileName = args[1];
            var methodName = args[2];
            var parameterCount = Convert.ToInt32(args[3]);

            var inputRoot = Path.GetDirectoryName(solutionPath);
            var excludePaths = SetupExcludePaths(inputRoot);
            var callerFinder = new CallerFinder(solutionPath, excludePaths);
            Console.Write(callerFinder.Find(fileName, methodName, parameterCount));
        }

        private static List<string> SetupExcludePaths(string inputRoot)
        {
            var excludePaths = new List<string>();
            var gendocExclude = CombinePath(inputRoot, ".gendoc_exclude");
            if (File.Exists(gendocExclude))
                excludePaths = File
                    .ReadAllLines(gendocExclude)
                    .Where(path => !string.IsNullOrWhiteSpace(path))
                    .Select(path => path.Trim())
                    .ToList();

            return excludePaths;
        }

        private static string CombinePath(string first, string second)
        {
            return $"{first.TrimEnd(Path.DirectorySeparatorChar)}{Path.DirectorySeparatorChar}{second.TrimStart(Path.DirectorySeparatorChar)}";
        }
    }
}