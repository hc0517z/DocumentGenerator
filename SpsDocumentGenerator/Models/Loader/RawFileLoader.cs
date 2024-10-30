using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SpsDocumentGenerator.Models.Checksum;
using SpsDocumentGenerator.Models.Content;

namespace SpsDocumentGenerator.Models.Loader;

public class RawFileLoader : FileLoader
{
    private readonly LineCounter _lineCounter = new();
    public RawFileLoader(ChecksumCreator checksumCreator, string[] ignoreDir, string[] ignoreFiles) : base(checksumCreator, ignoreDir, ignoreFiles)
    {
    }

    protected override FileItem CreateFileItem(string rootPath, int i, FileInfo info)
    {
        var fileItem = new RawFileItem
        {
            Index = i + 1,
            FileName = info.Name,
            FullPath = info.FullName,
            DirectoryPath = info.DirectoryName!,
            RelativeDirectoryPath = GetRelativeDirectoryPath(rootPath, info.DirectoryName!),
            Size = info.Length,
            Date = info.LastWriteTime,
            Checksum = checksumCreator.Create(info.FullName),
            LineCount = _lineCounter.CountCodeLines(info.FullName)
        };

        if (info.Extension == ".cs")
        {
            var remarksContents = ExtractRemarksContents(info.FullName);
            if (remarksContents.Any(s => !string.IsNullOrEmpty(s)))
            {
                fileItem.CsuName = string.Join(Environment.NewLine, remarksContents);
            }
        }

        return fileItem;
    }

    private List<string> ExtractRemarksContents(string filePath)
    {
        var code = File.ReadAllText(filePath);

        var syntaxTree = CSharpSyntaxTree.ParseText(code);
        var root = syntaxTree.GetCompilationUnitRoot();
        
        var remarksContents = root.DescendantNodes().OfType<TypeDeclarationSyntax>()
            .Select(syntax =>
            {
                var syntaxTrivia = syntax.GetLeadingTrivia();

                var remarksComment = syntaxTrivia
                    .Where(trivia => trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) ||
                                     trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia))
                    .Select(trivia => trivia.ToString().Trim())
                    .FirstOrDefault();

                if (!string.IsNullOrEmpty(remarksComment))
                {
                    var startTag = "/// <remarks>";
                    var endTag = "/// </remarks>";

                    var startIndex = remarksComment.IndexOf(startTag, StringComparison.Ordinal);
                    var endIndex = remarksComment.IndexOf(endTag, StringComparison.Ordinal);

                    if (startIndex >= 0 && endIndex > startIndex)
                        return remarksComment.Substring(startIndex + startTag.Length, endIndex - startIndex - startTag.Length)
                            .Replace("///", string.Empty).Trim();
                }

                return string.Empty;
            })
            .ToList();

        return remarksContents;
    }
}