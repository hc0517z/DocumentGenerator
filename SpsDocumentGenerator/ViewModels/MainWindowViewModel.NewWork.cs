using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.WindowsAPICodePack.Dialogs;
using SpsDocumentGenerator.Base;
using SpsDocumentGenerator.Models.Checksum;
using SpsDocumentGenerator.Models.Content;
using SpsDocumentGenerator.Models.Loader;

namespace SpsDocumentGenerator.ViewModels;

public partial class MainWindowViewModel
{
    [RelayCommand]
    public async Task NewExeAsync()
    {
        var loader = new ExeFileLoader(ChecksumCreatorFactory.Create(SelectHash.Value), _ignoreDir, _ignoreFiles);

        var result = await LoadFilesAsync("실행 파일 폴더 선택", loader);
        if (!result.IsFailure)
        {
            _content.ExeFiles = new List<FileItem>(result.Value);
            _renewService.RenewIndex(SelectIndex.Value, _content.ExeFiles);
            OnUpdatePartNumber();
            _renewService.ReplaceRootPath(_content.ExeFiles, _config["ReplaceRootPath:Exe"]);
            UpdateExeExpanderModels();
        }
    }

    private void UpdateExeExpanderModels()
    {
        _exeExpanderModels = new ObservableCollection<ExpanderModel<ExeFileItem>>();
        foreach (var item in _content.ExeFiles)
        {
            var exeFileItem = (ExeFileItem)item;
            var uiModel = _exeExpanderModels.FirstOrDefault(x => x.RelativeDirectoryPath == exeFileItem.RelativeDirectoryPath);
            if (uiModel == null)
            {
                uiModel = new ExpanderModel<ExeFileItem> { RelativeDirectoryPath = exeFileItem.RelativeDirectoryPath };
                _exeExpanderModels.Add(uiModel);
            }

            uiModel.Files.Add(exeFileItem);
        }

        OnPropertyChanged(nameof(ExeExpanderModels));
        NotifyCanExecuteChangedForCommand();
    }

    [RelayCommand]
    public async Task NewRawAsync()
    {
        var loader = new RawFileLoader(ChecksumCreatorFactory.Create(SelectHash.Value), _ignoreDir, _ignoreFiles);

        var result = await LoadFilesAsync("원시 파일 폴더 선택", loader);

        if (!result.IsFailure)
        {
            _content.RawFiles = new List<FileItem>(result.Value);
            _renewService.RenewIndex(SelectIndex.Value, _content.RawFiles);
            _renewService.ReplaceRootPath(_content.RawFiles, _config["ReplaceRootPath:Raw"]);
            UpdateRawExpanderModels();
        }
    }

    private void UpdateRawExpanderModels()
    {
        _rawExpanderModels = new ObservableCollection<ExpanderModel<RawFileItem>>();
        foreach (var item in _content.RawFiles)
        {
            var rawFileItem = (RawFileItem)item;
            var uiModel = _rawExpanderModels.FirstOrDefault(x => x.RelativeDirectoryPath == rawFileItem.RelativeDirectoryPath);
            if (uiModel == null)
            {
                uiModel = new ExpanderModel<RawFileItem> { RelativeDirectoryPath = rawFileItem.RelativeDirectoryPath };
                _rawExpanderModels.Add(uiModel);
            }

            uiModel.Files.Add(rawFileItem);
        }

        OnPropertyChanged(nameof(RawExpanderModels));
        NotifyCanExecuteChangedForCommand();
    }

    private async Task<Result<List<FileItem>>> LoadFilesAsync(string dialogTitle, FileLoader loader)
    {
        var dialog = new CommonOpenFileDialog { IsFolderPicker = true, Title = dialogTitle };

        if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
        {
            IsBusy = true;
            BusyContent = $"{dialogTitle}{Environment.NewLine} '{dialog.FileName}' 폴더의 내용을 읽는 중입니다.";

            var fileItems = await loader.LoadAsync(dialog.FileName);

            IsBusy = false;
            return Result<List<FileItem>>.Success(fileItems);
        }

        return Result<List<FileItem>>.Failure("Error");
    }
}