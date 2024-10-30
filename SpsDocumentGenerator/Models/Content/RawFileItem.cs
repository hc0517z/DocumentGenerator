using CommunityToolkit.Mvvm.ComponentModel;

namespace SpsDocumentGenerator.Models.Content;

public partial class RawFileItem : FileItem
{
    [ObservableProperty]
    private int _lineCount;

    [ObservableProperty]
    private string _csuName;
}