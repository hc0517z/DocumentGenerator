using CommunityToolkit.Mvvm.Input;
using SpsDocumentGenerator.Base;

namespace SpsDocumentGenerator.ViewModels;

public partial class MainWindowViewModel
{
    public bool CanRenewChecksum => _content.IsEmpty == false;

    public bool CanRenewIndex => _content.IsEmpty == false;

    public bool CanUpdatePartNumber => _content.IsEmptyExeFiles == false;

    public EnumSelection<HashKind> SelectHash { get; set; }
    public EnumSelection<IndexKind> SelectIndex { get; set; }

    private void InitRenew()
    {
        SelectHash = new EnumSelection<HashKind>(HashKind.Crc32);
        SelectIndex = new EnumSelection<IndexKind>(IndexKind.All);
    }

    [RelayCommand(CanExecute = nameof(CanRenewChecksum))]
    public async void OnRenewChecksum()
    {
        IsBusy = true;
        BusyContent = $"첵섬을 '{SelectHash.Value.ToString().ToUpper()}'(으)로 재계산 중입니다.";

        await _renewService.RenewChecksumAsync(SelectHash.Value, _content);

        IsBusy = false;
    }

    [RelayCommand(CanExecute = nameof(CanRenewIndex))]
    public void OnRenewIndex()
    {
        _renewService.RenewIndex(SelectIndex.Value, ref _content);
    }

    [RelayCommand(CanExecute = nameof(CanUpdatePartNumber))]
    public void OnUpdatePartNumber()
    {
        _renewService.UpdatePartNumber(_partNumberPrefix, _partNumberIndexFormat, _content.ExeFiles);
    }
}