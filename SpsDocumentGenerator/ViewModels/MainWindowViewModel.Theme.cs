using System.ComponentModel;
using SpsDocumentGenerator.Base;
using Wpf.Ui.Appearance;
using Wpf.Ui.Mvvm.Contracts;

namespace SpsDocumentGenerator.ViewModels;

public partial class MainWindowViewModel
{
    private readonly IThemeService _themeService;
    public EnumSelection<ThemeType> SelectTheme { get; set; }

    private void InitTheme()
    {
        var themeType = Theme.GetAppTheme();
        SelectTheme = new EnumSelection<ThemeType>(themeType);
        SelectTheme.PropertyChanged += SelectTheme_PropertyChanged;
    }

    private void SelectTheme_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        _themeService.SetTheme(SelectTheme.Value);
    }
}