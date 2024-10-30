using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using AxHWPCONTROLLib;
using HWPCONTROLLib;
using Microsoft.Win32;

namespace SpsDocumentGenerator.Models.Writer;

public abstract class HwpWriter : IDisposable
{
    private readonly AxHwpCtrl _axHwpCtrl = new();
    private readonly string _path;

    protected HwpWriter(string path)
    {
        _path = path;
    }

    protected abstract string BasePath { get; }

    private string ProgramDirectory => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public async Task CreateAsync(string fileName)
    {
        OnProgressChanged("SPS 문서를 생성하는 중입니다.");
        await Task.Run(() =>
        {
            EnsureAxHwpCtrl();
            var filePath = Path.Combine(_path, $"{fileName}.hwp");
            var directoryName = Path.GetDirectoryName(filePath);
            if (directoryName != null) Directory.CreateDirectory(directoryName);

            if (File.Exists(filePath)) File.Delete(filePath);
            File.Copy(BasePath, filePath);

            _axHwpCtrl.Open(filePath);
        });
    }

    private void EnsureAxHwpCtrl()
    {
        _axHwpCtrl.CreateControl();

        const string hncRoot = @"HKEY_Current_User\Software\HNC\HwpCtrl\Modules";
        Registry.SetValue(hncRoot, "FilePathCheckerModuleExample", ProgramDirectory + "\\FilePathCheckerModuleExample.dll");

        _axHwpCtrl.RegisterModule("FilePathCheckDLL", "FilePathCheckerModuleExample");
        _axHwpCtrl.Clear();
    }

    protected (DHwpAction act, DHwpParameterSet parameterSet) CreateActionAndParameterSet(string actId, string setId = "")
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

    protected bool FindText(string target)
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

    protected void InsertText(string text)
    {
        if (string.IsNullOrEmpty(text)) text = "-";

        var (act, parameterSet) = CreateActionAndParameterSet("InsertText");
        parameterSet.SetItem("Text", text);
        act.Execute(parameterSet);
    }

    protected bool ReplaceText(string target, string text)
    {
        if (!FindText(target)) return false;

        InsertText(text);
        return true;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing) _axHwpCtrl?.Dispose();
    }

    protected void Run(string actId)
    {
        ((DHwpAction)_axHwpCtrl.CreateAction(actId)).Run();
    }

    protected bool Save()
    {
        return _axHwpCtrl.Save();
    }

    public event EventHandler<string> ProgressChanged;

    protected void OnProgressChanged(string status)
    {
        ProgressChanged?.Invoke(this, status);
    }
}