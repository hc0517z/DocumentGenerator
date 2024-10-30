using System.Text;
using System.Windows;

namespace SpsDocumentGenerator;

public partial class App
{
    public static Encoding EucKrEncoding => Encoding.GetEncoding(51949);

    protected override void OnStartup(StartupEventArgs e)
    {
        new Bootstrap()
            .BuildConfiguration()
            .ConfigureServices()
            .EnsureWorkspace()
            .RegisterEncoding();

        base.OnStartup(e);
    }
}