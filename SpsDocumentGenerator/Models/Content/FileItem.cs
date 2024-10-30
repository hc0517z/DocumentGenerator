using System;
using CommunityToolkit.Mvvm.ComponentModel;
using SpsDocumentGenerator.Converters;

namespace SpsDocumentGenerator.Models.Content;

public abstract partial class FileItem : ObservableObject
{
    [ObservableProperty]
    private string _checksum = string.Empty;

    [ObservableProperty]
    private DateTime _date;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _directoryPath = string.Empty;

    [ObservableProperty]
    private string _fileName = string.Empty;

    [ObservableProperty]
    private string _fullPath = string.Empty;

    [ObservableProperty]
    private int _index;

    [ObservableProperty]
    private string _relativeDirectoryPath = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplaySize))]
    private long _size;
    public string DisplaySize => ByteConverter.FormatBytes(_size);

    [ObservableProperty]
    private string _version = "1.0";
}