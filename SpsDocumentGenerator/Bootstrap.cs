using System.IO;
using System.Text;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SpsDocumentGenerator.Models.Checksum;
using SpsDocumentGenerator.Models.Writer;
using SpsDocumentGenerator.Services;
using SpsDocumentGenerator.ViewModels;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Mvvm.Services;

namespace SpsDocumentGenerator;

public class Bootstrap
{
    /// <summary>
    ///     설정 인터페이스 필드
    /// </summary>
    private IConfiguration _configuration;

    public Bootstrap BuildConfiguration()
    {
        _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false, true)
            .AddEnvironmentVariables()
            .Build();

        return this;
    }

    public Bootstrap ConfigureServices()
    {
        Ioc.Default.ConfigureServices(
            new ServiceCollection()
                .AddSingleton(_ => _configuration)
                .AddSingleton<MainWindowViewModel>()
                .AddTransient<AppSettingsUpdateService>()
                .AddTransient<RenewService>()
                .AddTransient<WorkService>()
                .AddTransient<ExportService>()
                .AddTransient<ExplorerService>()
                .AddTransient<Md5ChecksumCreator>()
                .AddTransient<Crc32ChecksumCreator>()
                .AddSingleton<IThemeService, ThemeService>()
                .AddTransient(_ => new ExeHwpWriter(Paths.SpsExportDir))
                .AddTransient(_ => new RawHwpWriter(Paths.SpsExportDir))
                .BuildServiceProvider()
        );

        return this;
    }

    public Bootstrap EnsureWorkspace()
    {
        var workspace = Paths.Workspace;
        if (!Directory.Exists(workspace)) Directory.CreateDirectory(workspace);

        return this;
    }

    public Bootstrap RegisterEncoding()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        return this;
    }
}