using System;
using System.Runtime.InteropServices;

namespace SpsDocumentGenerator.Services;

public class ExplorerService
{
    [DllImport("shell32.dll")]
    private static extern IntPtr ShellExecute(
        IntPtr hwnd,
        string lpOperation,
        string lpFile,
        string lpParameters,
        string lpDirectory,
        int nShowCmd);

    public void Open(string path)
    {
        ShellExecute(IntPtr.Zero, "open", path, "", "", 1);
    }
}