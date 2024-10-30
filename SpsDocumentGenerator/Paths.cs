using System.IO;
using System.Reflection;

namespace SpsDocumentGenerator;

public static class Paths
{
    public static readonly string RuntimeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

    public static readonly string Workspace = Path.Combine(RuntimeDir, @"workspace");

    public static readonly string SpsExportDir = Path.Combine(RuntimeDir, @"hwp");

    public static readonly string BaseExeHwpFilePath = Path.Combine(RuntimeDir, @"Assets\Doc\SPS_Base_Exe.hwp");
    public static readonly string BaseRawHwpFilePath = Path.Combine(RuntimeDir, @"Assets\Doc\SPS_Base_Raw.hwp");
}