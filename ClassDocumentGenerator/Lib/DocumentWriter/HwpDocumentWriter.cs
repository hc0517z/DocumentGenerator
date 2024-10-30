using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AxHWPCONTROLLib;
using HWPCONTROLLib;
using Microsoft.Win32;

namespace ClassDocumentGenerator.Lib.DocumentWriter
{
    public class HwpDocumentWriter : IDocumentWriter
    {
        private readonly AxHwpCtrl _axHwpCtrl = new();
        private readonly string _path;

        public HwpDocumentWriter(string path)
        {
            _path = path;
        }

        private string ProgramDirectory => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public bool ExistsDocumentFile(string fileName)
        {
            var filePath = Path.Combine(_path, $"{fileName}.hwp");
            return File.Exists(filePath);
        }

        public string CreateDocumentFile(string fileName)
        {
            EnsureAxHwpCtrl();
            var basePath = Path.Combine(ProgramDirectory, "Base", "BaseDocumentHwp_Style.hwp");
            var filePath = Path.Combine(_path, $"{fileName}.hwp");
            var directoryName = Path.GetDirectoryName(filePath);
            if (directoryName != null) Directory.CreateDirectory(directoryName);

            if (File.Exists(filePath)) File.Delete(filePath);
            File.Copy(basePath, filePath);

            _axHwpCtrl.Open(filePath);
            return filePath;
        }

        public void WriteClassTable(ClassDetails classDetails)
        {
            MoveForInsertClassName();
            InsertText($"{classDetails.ClassName} 클래스");

            MoveForInsertClassDescription();
            InsertText(classDetails.Description);

            MoveForInsertCaption();
            InsertText($"{classDetails.ClassName} ");

            MoveBeginClassTable();
            InsertClassTableMembers(classDetails.ClassTableMembers);

            Run("MoveDocEnd");
        }

        public void WriteFunctionDetails(FunctionDetails details)
        {
            ReplaceText("FunctionName", $"{details.FunctionName}()");
            ReplaceText("FunctionCaption", $"{details.FunctionName}()");
            ReplaceText(nameof(details.FunctionDescription), details.FunctionDescription);

            var parentFunction = details.ParentFunction;
            if (string.IsNullOrEmpty(parentFunction)) parentFunction = details.FunctionName;

            ReplaceText(nameof(details.ParentFunction), parentFunction);

            if (IsDashOrOneLine(parentFunction)) Run("StyleShortcut6");

            ReplaceText(nameof(details.SourceFileName), details.SourceFileName);
            
            ReplaceText(nameof(details.Inputs), details.InputsString);
            if (IsDashOrOneLine(details.InputsString)) Run("StyleShortcut6");
            
            ReplaceText(nameof(details.Output), details.OutputString);
            if (IsDashOrOneLine(details.OutputString)) Run("StyleShortcut6");
            
            ReplaceText($"#{nameof(details.Process)}", details.Process);

            Run("MoveDocEnd");
        }

        private static bool IsDashOrOneLine(string parentFunction)
        {
            var isDashOrOneLine = parentFunction.Equals("-") || !parentFunction.Contains("\r\n");
            return isDashOrOneLine;
        }

        public bool Save()
        {
            Run("MoveDocBegin");
            if (FindText("FunctionName"))
            {
                Run("MoveSelViewDown");
                Run("MoveSelTopLevelEnd");
                Run("MoveSelTopLevelEnd");
                Run("Delete");
            }

            return _axHwpCtrl.Save();
        }

        public bool Merge(Dictionary<string, List<string>> groups)
        {
            var dirName = _path;
            EnsureAxHwpCtrl();
            var basePath = Path.Combine(ProgramDirectory, "Base", "BaseDocumentHwp_Group.hwp");

            if (!Directory.Exists(dirName)) Directory.CreateDirectory(dirName);

            foreach (var (groupsKey, paths) in groups)
            {
                var fileName = System.Text.RegularExpressions.Regex.Replace(groupsKey, @"[\\/:*?""<>|]", "_");
                var groupFileName = Path.Combine(dirName, $"{fileName}.hwp");
                File.Copy(basePath, groupFileName);
                Merge(groupFileName, paths);

                Console.WriteLine($"Merged \"{groupFileName}\"...");
            }

            return true;
        }

        public bool Merge(string path, List<string> paths)
        {
            var open = _axHwpCtrl.Open(path);
            if (!open) return false;

            foreach (var classDocPath in paths)
            {
                Run("MoveDocEnd");
                var (act, parameterSet) = CreateActionAndParameterSet("InsertFile");
                parameterSet.SetItem("FileName", classDocPath);
                parameterSet.SetItem("KeepSection", false);
                parameterSet.SetItem("KeepCharshape", true);
                parameterSet.SetItem("KeepParashape", true);
                parameterSet.SetItem("KeepStyle", true);
                act.Execute(parameterSet);
            }

            Save();
            return true;
        }

        private void EnsureAxHwpCtrl()
        {
            _axHwpCtrl.CreateControl();

            const string hncRoot = @"HKEY_Current_User\Software\HNC\HwpCtrl\Modules";
            Registry.SetValue(hncRoot, "FilePathCheckerModuleExample", ProgramDirectory + "\\FilePathCheckerModuleExample.dll");

            _axHwpCtrl.RegisterModule("FilePathCheckDLL", "FilePathCheckerModuleExample");
            _axHwpCtrl.Clear();
        }

        private void MoveForInsertClassName()
        {
            Run("MoveDocBegin");
            Run("MoveDown");
            Run("MoveLineEnd");
        }

        private void InsertClassTableMembers(List<ClassTableMember> members)
        {
            var variableMembers = members.Where(member => member.MemberType == ClassTableMemberType.Variable).ToList();
            InsertMembers(variableMembers);

            MoveFunctionCell();

            var functionMembers = members.Where(member => member.MemberType == ClassTableMemberType.Function).ToList();
            InsertMembers(functionMembers);

            Run("TableColPageUp");
            CleanEmptyCell(variableMembers);
            CleanEmptyCell(functionMembers);

            if (variableMembers.Count + functionMembers.Count == 0) DeleteEmptyTable();
        }

        private void DeleteEmptyTable()
        {
            Run("MoveDown");
            Run("MoveSelUp");
            Run("Delete");
        }

        private void CleanEmptyCell(List<ClassTableMember> variableMembers)
        {
            Run("TableColEnd");
            Run("TableLowerCell");

            for (var i = 0; i < variableMembers.Count; i++) Run("TableLowerCell");

            DeleteTableRow();
            if (variableMembers.Count == 0) DeleteTableRow();
        }

        private void DeleteTableRow()
        {
            var (act, parameterSet) = CreateActionAndParameterSet("TableDeleteRow", "TableDeleteLine");
            parameterSet.SetItem("Type", 0);
            act.Execute(parameterSet);
        }

        private (DHwpAction act, DHwpParameterSet parameterSet) CreateActionAndParameterSet(string actId, string setId = "")
        {
            var act = (DHwpAction)_axHwpCtrl.CreateAction(actId);

            DHwpParameterSet parameterSet;
            if (string.IsNullOrEmpty(setId))
                parameterSet = (DHwpParameterSet)act.CreateSet();
            else
                parameterSet = (DHwpParameterSet)_axHwpCtrl.CreateSet(setId);
            act.GetDefault(parameterSet);
            return (act, parameterSet);
        }

        private void MoveFunctionCell()
        {
            Run("TableColBegin");
            Run("TableLeftCell");
            Run("TableLeftCell");
            Run("TableLeftCell");
            Run("TableLeftCell");
            Run("TableLowerCell");
            Run("TableLowerCell");
            Run("TableRightCell");
        }

        private void InsertMembers(List<ClassTableMember> variableMembers)
        {
            for (var i = 0; i < variableMembers.Count; i++)
            {
                var member = variableMembers[i];
                InsertClassTableMember(member);

                if (i != variableMembers.Count - 1)
                    NextLineClassTable();
            }
        }

        private void NextLineClassTable()
        {
            Run("TableAppendRow");
            Run("TableLeftCell");
            Run("TableLeftCell");
        }

        private void InsertClassTableMember(ClassTableMember classTableMember)
        {
            InsertText(classTableMember.TypeAndReturnType);
            Run("TableRightCell");
            InsertText(classTableMember.Name);
            Run("TableRightCell");
            InsertText(classTableMember.FunctionDescription);
        }

        private void MoveBeginClassTable()
        {
            Run("MoveDown");
            Run("MoveDown");
            Run("TableColBegin");
            Run("TableLeftCell");
            Run("TableLeftCell");
            Run("TableLeftCell");
            Run("TableLeftCell");
            Run("TableLowerCell");
            Run("TableRightCell");
        }

        private void InsertText(string text)
        {
            if (string.IsNullOrEmpty(text)) text = "-";

            var (act, parameterSet) = CreateActionAndParameterSet("InsertText");
            parameterSet.SetItem("Text", text);
            act.Execute(parameterSet);
        }

        private bool FindText(string target)
        {
            var (act, parameterSet) = CreateActionAndParameterSet("ForwardFind", "FindReplace");
            parameterSet.SetItem("FindString", target);
            parameterSet.SetItem("MatchCase", true);
            parameterSet.SetItem("WholeWordOnly", false);
            parameterSet.SetItem("IgnoreMessage", true);
            parameterSet.SetItem("FindType", false);
            var find = act.Execute(parameterSet);
            return find == 1;
        }

        private bool ReplaceText(string target, string text)
        {
            Run("MoveDocBegin");
            if (!FindText(target)) return false;

            InsertText(text);
            return true;
        }

        private void MoveForInsertCaption()
        {
            Run("MoveDown");
            Run("MoveDown");
            Run("ShapeObjAttachCaption");
            Run("MoveNextWord");
            Run("MoveNextWord");
        }

        private void MoveForInsertClassDescription()
        {
            Run("MoveDown");
            Run("MoveLineEnd");
        }

        private void Run(string actId)
        {
            ((DHwpAction)_axHwpCtrl.CreateAction(actId)).Run();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing) _axHwpCtrl?.Dispose();
        }
    }
}