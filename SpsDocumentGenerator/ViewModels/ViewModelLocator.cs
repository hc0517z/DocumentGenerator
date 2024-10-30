using CommunityToolkit.Mvvm.DependencyInjection;

namespace SpsDocumentGenerator.ViewModels;

public class ViewModelLocator
{
    public MainWindowViewModel MainWindow => Ioc.Default.GetRequiredService<MainWindowViewModel>();
}