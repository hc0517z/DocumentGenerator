using System;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using SpsDocumentGenerator.Controls;

namespace SpsDocumentGenerator.ViewModels;

public partial class MainWindowViewModel
{
    public string IgnoreDirStr
    {
        get => string.Join(Environment.NewLine, _ignoreDir);
        set
        {
            IgnoreDir = ParseIgnore(value, Environment.NewLine);
            OnPropertyChanged();
        }
    }

    public string IgnoreFileStr
    {
        get => string.Join(Environment.NewLine, _ignoreFiles);
        set
        {
            IgnoreFiles = ParseIgnore(value, Environment.NewLine);
            OnPropertyChanged();
        }
    }

    private void InitIgnore()
    {
        _ignoreDir = ParseIgnore(_config["Ignore:Directories"]);
        _ignoreFiles = ParseIgnore(_config["Ignore:Files"]);
    }

    private string[] ParseIgnore(string ignoreStr, string split = ";")
    {
        if (string.IsNullOrEmpty(ignoreStr))
            return Array.Empty<string>();

        return ignoreStr.Split(split).Where(x => !string.IsNullOrEmpty(x)).ToArray();
    }

    [RelayCommand]
    public void OnApplyIgnoreFile()
    {
        Mbox.ShowDialog("무시할 파일 목록 적용", "설정 파일에 목록이 저장됩니다. 적용하시겠습니까?", "예", "아니오", () =>
        {
            var json = string.Join(";", _ignoreFiles);
            _appSettingsUpdateService.UpdateAppSetting("Ignore:Files", json);

            _content.ApplyIgnoreFiles(_ignoreFiles);
            UpdateExeExpanderModels();
            UpdateRawExpanderModels();
            OnRenewIndex();
            OnUpdatePartNumber();
        }, () => { });
    }

    [RelayCommand]
    public void OnApplyIgnoreDirectories()
    {
        Mbox.ShowDialog("무시할 폴더 목록 적용", "설정 파일에 목록이 저장됩니다. 적용하시겠습니까?", "예", "아니오", () =>
        {
            var json = string.Join(";", _ignoreDir);
            _appSettingsUpdateService.UpdateAppSetting("Ignore:Directories", json);

            _content.ApplyIgnoreDirectories(_ignoreDir);
            UpdateExeExpanderModels();
            UpdateRawExpanderModels();
            OnRenewIndex();
            OnUpdatePartNumber();
        }, () => { });
    }
}