using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SpsDocumentGenerator.ViewModels;

public partial class ExpanderModel<T> : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<T> _files = new();

    [ObservableProperty]
    private string _relativeDirectoryPath = string.Empty;
}