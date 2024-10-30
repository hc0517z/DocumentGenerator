using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.MSBuild;

namespace CallerFinder
{
    public class CallerFinder
    {
        private readonly List<string> _excludePaths;
        private readonly string _solutionPath;

        public CallerFinder(string solutionPath, List<string> excludePaths)
        {
            _solutionPath = solutionPath;
            _excludePaths = excludePaths;
        }

        public string Find(string fileName, string methodName, int parameterCount)
        {
            MSBuildLocator.RegisterDefaults();
            var msWorkspace = MSBuildWorkspace.Create();

            var solution = msWorkspace.OpenSolutionAsync(_solutionPath).Result;
            var documents = GetDocumentsExcludeList(solution).ToImmutableHashSet();

            ISymbol methodSymbol = null;
            foreach (var document in documents.Where(document => string.Equals(document.Name, fileName, StringComparison.CurrentCultureIgnoreCase)))
            {
                var model = document.GetSemanticModelAsync().Result;
                var methodInvocation = document.GetSyntaxRootAsync().Result;
                MethodDeclarationSyntax node = null;
                try
                {
                    if (methodInvocation != null)
                        node = methodInvocation
                            .DescendantNodes()
                            .OfType<MethodDeclarationSyntax>()
                            .FirstOrDefault(syntax => syntax.Identifier.ToString() == methodName &&
                                                      syntax.ParameterList.Parameters.Count == parameterCount);

                    if (node == null)
                        continue;
                }
                catch (Exception)
                {
                    // Swallow the exception of type cast. 
                    // Could be avoided by a better filtering on above linq.
                    continue;
                }

                if (model != null) methodSymbol = model.GetSymbolInfo(node).Symbol ?? model.GetDeclaredSymbol(node);
                break;
            }

            if (methodSymbol == null) return string.Empty;
            var callingSymbols = SymbolFinder.FindCallersAsync(methodSymbol, solution, documents)
                .Result
                .Select(symbolCallerInfo =>
                {
                    var displayString = symbolCallerInfo.CallingSymbol.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat);

                    // 괄호안 문자열 제거
                    var replace = Regex.Replace(displayString, @"\([^)]*\)", "");
                    return replace;
                })
                .Distinct()
                .ToList();
            return string.Join("\r\n", callingSymbols);
        }

        private IEnumerable<Document> GetDocumentsExcludeList(Solution solution)
        {
            var documents = new List<Document>();
            foreach (var solutionProject in solution.Projects)
            {
                foreach (Document projectDocument in solutionProject.Documents)
                {
                    var solutionDirectory = Path.GetDirectoryName(_solutionPath);
                    var isContain = _excludePaths.Any(excludePath =>
                    {
                        if (projectDocument.FilePath == null || solutionDirectory == null) return false;
                        var excludeSolutionPath = projectDocument.FilePath.Replace(solutionDirectory, string.Empty);
                        return excludeSolutionPath.Contains(excludePath);
                    });
                
                    if (isContain) continue;
                    documents.Add(projectDocument);
                }
            }

            return documents;
        }
    }
}