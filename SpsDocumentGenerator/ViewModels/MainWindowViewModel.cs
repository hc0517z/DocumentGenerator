using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Configuration;
using SpsDocumentGenerator.Models.Content;
using SpsDocumentGenerator.Services;
using Wpf.Ui.Mvvm.Contracts;

namespace SpsDocumentGenerator.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly AppSettingsUpdateService _appSettingsUpdateService;

    private readonly IConfiguration _config;
    private readonly ExplorerService _explorerService;
    private readonly ExportService _exportService;

    private readonly string _partNumberPrefix;
    private readonly string _partNumberIndexFormat;
    private readonly RenewService _renewService;
    private readonly WorkService _workService;

    [ObservableProperty]
    private string _busyContent;

    private SpsContent _content;

    [ObservableProperty]
    private ObservableCollection<ExpanderModel<ExeFileItem>> _exeExpanderModels;

    [ObservableProperty]
    private string[] _ignoreDir;

    [ObservableProperty]
    private string[] _ignoreFiles;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private ObservableCollection<ExpanderModel<RawFileItem>> _rawExpanderModels;

    public MainWindowViewModel(IConfiguration config,
        AppSettingsUpdateService appSettingsUpdateService,
        RenewService checksumRenewService,
        WorkService workService,
        ExportService exportService,
        ExplorerService explorerService,
        IThemeService themeService)
    {
        _config = config;
        _renewService = checksumRenewService;
        _workService = workService;
        _exportService = exportService;
        _explorerService = explorerService;
        _themeService = themeService;
        _appSettingsUpdateService = appSettingsUpdateService;

        _content = new SpsContent();

        InitRenew();
        InitIgnore();
        _partNumberPrefix = config["PartNumberPrefix"];
        _partNumberIndexFormat = config["PartNumberIndexFormat"];
        InitTheme();
    }

    private void NotifyCanExecuteChangedForCommand()
    {
        RenewChecksumCommand.NotifyCanExecuteChanged();
        RenewIndexCommand.NotifyCanExecuteChanged();
        SaveWorkCsvCommand.NotifyCanExecuteChanged();
        UpdateExeWorkCsvCommand.NotifyCanExecuteChanged();
        UpdateRawWorkCsvCommand.NotifyCanExecuteChanged();
        ExportCsvCommand.NotifyCanExecuteChanged();
        ExportSpsCommand.NotifyCanExecuteChanged();
        UpdatePartNumberCommand.NotifyCanExecuteChanged();
    }
}