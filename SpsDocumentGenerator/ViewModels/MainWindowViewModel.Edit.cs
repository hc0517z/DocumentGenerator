using System;
using System.IO;
using CommunityToolkit.Mvvm.Input;
using Microsoft.WindowsAPICodePack.Dialogs;
using SpsDocumentGenerator.Controls;

namespace SpsDocumentGenerator.ViewModels;

public partial class MainWindowViewModel
{
    public bool CanSaveWork => _content.IsEmpty == false;

    public bool CanUpdateExeWork => _content.IsEmptyExeFiles == false;

    public bool CanUpdateRawWork => _content.IsEmptyRawFiles == false;

    [RelayCommand]
    public void OnOpenWorkspaceWithExplorer()
    {
        _explorerService.Open(Paths.Workspace);
    }

    [RelayCommand(CanExecute = nameof(CanSaveWork))]
    public void OnSaveWorkCsv()
    {
        var saveDir = Path.Combine(Paths.Workspace, $"{DateTime.Now:yyMMdd}");

        if (!Directory.Exists(saveDir)) Directory.CreateDirectory(saveDir);

        var exeSavePath = Path.Combine(saveDir, $"ExeWork_{DateTime.Now:yyMMdd_HHmmss}.csv");
        var writeExeWorkCsv = _workService.WriteExeWorkCsv(exeSavePath, _content);

        var rawSavePath = Path.Combine(saveDir, $"RawWork_{DateTime.Now:yyMMdd_HHmmss}.csv");
        var writeRawWorkCsv = _workService.WriteRawWorkCsv(rawSavePath, _content);

        if (writeExeWorkCsv && writeRawWorkCsv)
            Mbox.Show($"작업 파일이 저장되었습니다.{Environment.NewLine}{saveDir}");
        else
            Mbox.Show($"작업 파일 저장에 실패했습니다. ({saveDir})");
    }

    [RelayCommand(CanExecute = nameof(CanUpdateExeWork))]
    public void OnUpdateExeWorkCsv()
    {
        var dialog = new CommonOpenFileDialog { Title = "실행 파일 작업본 선택/적용 (구분/부품번호/기능설명)", InitialDirectory = Paths.Workspace };
        if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
        {
            var readExeWorkCsv = _workService.ReadExeWorkCsv(dialog.FileName);
            var updateExeWork = _workService.UpdateExeWork(_content.ExeFiles, readExeWorkCsv);
            Mbox.Show(
                updateExeWork ? $"{Path.GetFileName(dialog.FileName)} 파일이 적용되었습니다." : $"{Path.GetFileName(dialog.FileName)} 파일 적용에 실패했습니다. "
            );
        }
    }

    [RelayCommand(CanExecute = nameof(CanUpdateRawWork))]
    public void OnUpdateRawWorkCsv()
    {
        var dialog = new CommonOpenFileDialog { Title = "원시 파일 작업본 선택/적용 (기능설명)", InitialDirectory = Paths.Workspace };
        if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
        {
            var readRawWorkCsv = _workService.ReadRawWorkCsv(dialog.FileName);
            var updateRawWork = _workService.UpdateRawWork(_content.RawFiles, readRawWorkCsv);
            Mbox.Show(
                updateRawWork ? $"{Path.GetFileName(dialog.FileName)} 파일이 적용되었습니다." : $"{Path.GetFileName(dialog.FileName)} 파일 적용에 실패했습니다. "
            );
        }
    }
}