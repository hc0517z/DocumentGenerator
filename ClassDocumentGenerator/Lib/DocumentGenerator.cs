using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using ClassDocumentGenerator.Lib.DocumentWriter;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ClassDocumentGenerator.Lib
{
    public class DocumentGenerator : CSharpSyntaxWalker
    {
        private readonly ClassDetails _classDetails = new();
        private readonly bool _findCaller;
        private readonly List<FunctionDetails> _functionDetails = new();
        private readonly List<(string, string)> _groupNameAndCreatedFiles = new();
        private readonly Accessibilities _ignoreMemberAccessibilities;
        private readonly string _inputFileName;
        private readonly string _solutionPath;

        private readonly IDocumentWriter _writer;
        private int _nestingDepth;

        public DocumentGenerator(
            IDocumentWriter writer,
            string inputFileName,
            string solutionPath = "",
            bool findCaller = false,
            Accessibilities ignoreMemberAccessibilities = Accessibilities.None)
        {
            _writer = writer;
            _inputFileName = inputFileName;
            _findCaller = findCaller;
            _solutionPath = solutionPath;
            _ignoreMemberAccessibilities = ignoreMemberAccessibilities;
        }

        public List<(string, string)> GenerateAndReturnGroupNameAndFileNames(SyntaxNode root)
        {
            Visit(root);
            return _groupNameAndCreatedFiles;
        }

        // public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
        // {
        //     VisitTypeDeclaration(node, () => base.VisitInterfaceDeclaration(node));
        // }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            VisitTypeDeclaration(node, () => base.VisitClassDeclaration(node));
        }

        public override void VisitRecordDeclaration(RecordDeclarationSyntax node)
        {
            VisitTypeDeclaration(node, () => base.VisitRecordDeclaration(node));
        }

        public override void VisitEventFieldDeclaration(EventFieldDeclarationSyntax node)
        {
            if (IsIgnoreMember(node.Modifiers)) return;
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            if (IsIgnoreMember(node.Modifiers)) return;
            VisitTrivia(node);
        }

        private void VisitTypeDeclaration(TypeDeclarationSyntax node, Action visitBase)
        {
            if (SkipInnerTypeDeclaration()) return;

            ResetDocumentDetails();
            VisitTrivia(node);

            _nestingDepth++;
            visitBase();
            _nestingDepth--;

            WriteDocument();
        }

        private void WriteDocument()
        {
            if (_classDetails.IsEmpty) return;

            string createFileName;
            if (string.IsNullOrEmpty(_classDetails.ClassName) || _writer.ExistsDocumentFile(_classDetails.ClassName))
                createFileName = _inputFileName;
            else
                createFileName = _classDetails.ClassName;

            var createdFilePath = _writer.CreateDocumentFile(createFileName.ConvertFileNameToUsableString());
            _writer.WriteClassTable(_classDetails);

            foreach (var detail in _functionDetails) _writer.WriteFunctionDetails(detail);
            _writer.Save();

            _groupNameAndCreatedFiles.Add((_classDetails.GroupName, createdFilePath));
        }

        private void ResetDocumentDetails()
        {
            _classDetails.Reset();
            _functionDetails.Clear();
        }

        public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            if (IsIgnoreMember(node.Modifiers)) return;
            VisitTrivia(node);
        }

        private void VisitTrivia(CSharpSyntaxNode node)
        {
            foreach (var trivia in node.GetLeadingTrivia().ToList())
            {
                if (!trivia.HasStructure) continue;

                foreach (var syntaxNode in trivia.GetStructure()!.ChildNodes().Where(syntaxNode => syntaxNode is XmlElementSyntax))
                {
                    var elementSyntax = (XmlElementSyntax)syntaxNode;
                    if (!elementSyntax.HasStartTagContent()) continue;
                    var tagContent = elementSyntax.GetTagContent();

                    AddOrFillClassDeclarationSyntax(node, tagContent);
                    AddOrFillRecordDeclarationSyntax(node, tagContent);
                    AddOrFillFieldDeclarationSyntax(node, tagContent);
                    AddOrFillPropertyDeclarationSyntax(node, tagContent);
                    AddOrFillMethodDeclarationSyntax(node, tagContent);
                }
            }
        }

        private void AddOrFillMethodDeclarationSyntax(CSharpSyntaxNode node, TagContent tagContent)
        {
            if (node is not MethodDeclarationSyntax methodDeclarationSyntax) return;

            if (tagContent.Tag == CommentTag.Summary)
                _classDetails.ClassTableMembers.Add(new ClassTableMember
                {
                    MemberType = ClassTableMemberType.Function,
                    FunctionDescription = tagContent.Content,
                    Name = methodDeclarationSyntax.Identifier.ToString(),
                    TypeAndReturnType = methodDeclarationSyntax.ReturnType.ToString()
                });


            var functionName = methodDeclarationSyntax.Identifier.ToString();
            var functionDetails = new FunctionDetails { FunctionName = functionName };

            string parameterDescription = null;
            string returnDescription = null;
            switch (tagContent.Tag)
            {
                case CommentTag.Summary:
                    functionDetails.FunctionDescription = tagContent.Content;
                    break;
                case CommentTag.Remarks:
                    functionDetails.Process = tagContent.Content;
                    break;
                case CommentTag.Code:
                    //functionDetails.ParentFunction = tagContent.Content;
                    break;
                case CommentTag.Param:
                    parameterDescription = tagContent.Content;
                    break;
                case CommentTag.Returns:
                    returnDescription = tagContent.Content;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            functionDetails.SourceFileName = _inputFileName;
            // var args = methodDeclarationSyntax.ParameterList.Parameters.Select(p => $"{p.Type} {p.Identifier}");

            functionDetails.ParameterFullString = methodDeclarationSyntax.ParameterList.Parameters.ToFullString();
            if (parameterDescription != null)
            {
                var parameterSyntax =
                    methodDeclarationSyntax.ParameterList.Parameters.First(syntax => syntax.Identifier.ToString() == tagContent.ParameterName);
                functionDetails.Inputs.Add(new DocumentWriter.ParameterInfo
                    { Description = parameterDescription, Name = parameterSyntax.Identifier.ToString(), Type = parameterSyntax.Type!.ToString() });
            }
            // functionDetails.Inputs = string.Join(", ", args);

            if (returnDescription != null)
            {
                functionDetails.Output.Description = returnDescription;
                functionDetails.Output.Type = methodDeclarationSyntax.ReturnType.ToString();
            }

            if (_findCaller && tagContent.Tag == CommentTag.Summary) FindCaller(functionDetails, functionName, methodDeclarationSyntax);

            var find = _functionDetails.Find(details => details.Equals(functionDetails));
            if (find != null)
            {
                _functionDetails.Remove(find);
                functionDetails.Merge(find);
            }

            _functionDetails.Add(functionDetails);
        }

        private void FindCaller(FunctionDetails functionDetails, string functionName, MethodDeclarationSyntax methodDeclarationSyntax)
        {
            Console.Write($"\tFind Callers : [{functionName}] Function...");

            var fileName = functionDetails.SourceFileName;
            var methodName = functionName;
            var parameterCount = methodDeclarationSyntax.ParameterList.Parameters.Count;

            var app = new Process();
            var programDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var callerFinderPath = Path.Combine(programDirectory, "CallerFinder.exe");
            app.StartInfo.FileName = callerFinderPath;
            app.StartInfo.Arguments = $"{_solutionPath} {fileName} {methodName} {parameterCount}";
            app.StartInfo.UseShellExecute = false;
            app.StartInfo.RedirectStandardOutput = true;
            app.Start();
            var output = app.StandardOutput.ReadToEnd();
            app.WaitForExit();

            if (!string.IsNullOrEmpty(output))
            {
                functionDetails.ParentFunction = output;
                Console.WriteLine("\tFound !!!");
            }
            else
            {
                functionDetails.ParentFunction = "-";
                Console.WriteLine("\tNot found ~~~");
            }
        }

        private void AddOrFillPropertyDeclarationSyntax(CSharpSyntaxNode node, TagContent tagContent)
        {
            if (node is not PropertyDeclarationSyntax propertyDeclarationSyntax) return;
            if (tagContent.Tag != CommentTag.Summary) return;

            _classDetails.ClassTableMembers.Add(new ClassTableMember
            {
                MemberType = ClassTableMemberType.Variable,
                FunctionDescription = tagContent.Content,
                Name = propertyDeclarationSyntax.Identifier.ToString(),
                TypeAndReturnType = propertyDeclarationSyntax.Type.ToString()
            });
        }

        private void AddOrFillFieldDeclarationSyntax(CSharpSyntaxNode node, TagContent tagContent)
        {
            if (node is not FieldDeclarationSyntax fieldDeclarationSyntax) return;
            if (tagContent.Tag != CommentTag.Summary) return;

            foreach (var field in fieldDeclarationSyntax.Declaration.Variables)
                _classDetails.ClassTableMembers.Add(new ClassTableMember
                {
                    MemberType = ClassTableMemberType.Variable,
                    FunctionDescription = tagContent.Content,
                    Name = field.Identifier.ToString(),
                    TypeAndReturnType = fieldDeclarationSyntax.Declaration.Type.ToString()
                });
        }

        private void AddOrFillClassDeclarationSyntax(CSharpSyntaxNode node, TagContent tagContent)
        {
            if (node is not ClassDeclarationSyntax classDeclarationSyntax) return;
            switch (tagContent.Tag)
            {
                case CommentTag.Summary:
                    var typeName = TypeNameText.From(classDeclarationSyntax);
                    _classDetails.ClassName = typeName.Identifier;
                    _classDetails.Description = tagContent.Content;
                    break;
                case CommentTag.Remarks:
                    _classDetails.GroupName = tagContent.Content;
                    break;
            }
        }

        private void AddOrFillRecordDeclarationSyntax(CSharpSyntaxNode node, TagContent tagContent)
        {
            if (node is not RecordDeclarationSyntax recordDeclarationSyntax) return;
            switch (tagContent.Tag)
            {
                case CommentTag.Summary:
                    var typeName = TypeNameText.From(recordDeclarationSyntax);
                    _classDetails.ClassName = typeName.Identifier;
                    _classDetails.Description = tagContent.Content;
                    break;
                case CommentTag.Remarks:
                    _classDetails.GroupName = tagContent.Content;
                    break;
            }
        }

        public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            if (IsIgnoreMember(node.Modifiers)) return;
            VisitTrivia(node);
        }

        private bool IsIgnoreMember(SyntaxTokenList modifiers)
        {
            if (_ignoreMemberAccessibilities == Accessibilities.None) return false;

            var tokenKinds = modifiers.Select(x => x.Kind()).ToArray();

            if (_ignoreMemberAccessibilities.HasFlag(Accessibilities.ProtectedInternal)
                && tokenKinds.Contains(SyntaxKind.ProtectedKeyword)
                && tokenKinds.Contains(SyntaxKind.InternalKeyword))
                return true;

            if (_ignoreMemberAccessibilities.HasFlag(Accessibilities.Public)
                && tokenKinds.Contains(SyntaxKind.PublicKeyword))
                return true;

            if (_ignoreMemberAccessibilities.HasFlag(Accessibilities.Protected)
                && tokenKinds.Contains(SyntaxKind.ProtectedKeyword))
                return true;

            if (_ignoreMemberAccessibilities.HasFlag(Accessibilities.Internal)
                && tokenKinds.Contains(SyntaxKind.InternalKeyword))
                return true;

            if (_ignoreMemberAccessibilities.HasFlag(Accessibilities.Private)
                && tokenKinds.Contains(SyntaxKind.PrivateKeyword))
                return true;

            return false;
        }

        private bool SkipInnerTypeDeclaration()
        {
            return _nestingDepth > 0;
        }
    }
}