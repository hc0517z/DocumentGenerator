using CommunityToolkit.Mvvm.ComponentModel;

namespace SpsDocumentGenerator.Models.Content;

public partial class ExeFileItem : FileItem
{
    [ObservableProperty]
    private string _partNumber = string.Empty;

    [ObservableProperty]
    private string _section = string.Empty;
}