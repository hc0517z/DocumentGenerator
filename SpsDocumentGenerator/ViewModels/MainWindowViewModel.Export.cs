using System;
using System.IO;
using CommunityToolkit.Mvvm.Input;
using Microsoft.WindowsAPICodePack.Dialogs;
using SpsDocumentGenerator.Controls;

namespace SpsDocumentGenerator.ViewModels;

public partial class MainWindowViewModel
{
    public bool CanExportCsv => _content.IsEmpty == false;

    public bool CanExportSps => _content.IsEmpty == false;

    [RelayCommand(CanExecute = nameof(CanExportCsv))]
    public void OnExportCsv()
    {
        var defaultFileName = $"CSV_{DateTime.Now:yyMMdd_HHmmss}.csv";
        var dialog = new CommonSaveFileDialog
            { Title = "CSV 내보내기", DefaultFileName = defaultFileName, Filters = { new CommonFileDialogFilter("CSV 파일", "*.csv") } };

        if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
        {
            IsBusy = true;
            BusyContent = "CSV로 내보내는 중입니다.";
            var exportCsv = _exportService.ExportCsv(dialog.FileName, _content);
            IsBusy = false;

            if (exportCsv.IsFailure)
                Mbox.Show($"CSV 내보내기에 실패했습니다. {exportCsv.ErrorMessage}");
            else
                Mbox.ShowDialog("CSV 내보내기 완료", "CSV 내보내기가 완료되었습니다.\r\n해당 폴더로 이동하시겠습니까?", "예", "아니오", () =>
                {
                    var directoryName = Path.GetDirectoryName(dialog.FileName);
                    _explorerService.Open(directoryName);
                }, () => { });
        }
    }

    [RelayCommand(CanExecute = nameof(CanExportSps))]
    public async void OnExportSps()
    {
        var dateString = DateTime.Now.ToString("yyMMdd_HHmmss");

        if (_content.IsEmpty)
        {
            Mbox.Show("SPS 내보내기를 할 수 없습니다.\r\n내용이 비어있습니다.");
            return;
        }

        IsBusy = true;

        _exportService.ExportSpsProgressChanged += ExportServiceOnExportSpsProgressChanged;
        var result = await _exportService.ExportSps(_content, dateString);
        _exportService.ExportSpsProgressChanged -= ExportServiceOnExportSpsProgressChanged;

        IsBusy = false;

        if (result.IsFailure)
            Mbox.Show($"SPS 내보내기에 실패했습니다. {result.ErrorMessage}");
        else
            Mbox.ShowDialog("SPS 내보내기 완료", "SPS 내보내기가 완료되었습니다.\r\n해당 폴더로 이동하시겠습니까?", "예", "아니오",
                () => { _explorerService.Open(Path.Combine(Paths.SpsExportDir, dateString)); }, () => { });
    }

    private void ExportServiceOnExportSpsProgressChanged(string message)
    {
        BusyContent = message;
    }
}