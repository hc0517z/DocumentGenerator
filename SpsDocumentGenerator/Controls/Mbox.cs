using System;
using System.Linq;
using System.Windows;
using SpsDocumentGenerator.Views;
using MessageBox = Wpf.Ui.Controls.MessageBox;

namespace SpsDocumentGenerator.Controls;

public static class Mbox
{
    public static void Show(string text)
    {
        var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
        new MessageBox { ShowTitle = false, ShowFooter = false, Owner = mainWindow }.Show("-", text);
    }

    public static bool? ShowDialog(string title, string txt, string btnLeft, string btnRight, Action leftAction, Action rightAction)
    {
        var messageBox = new MessageBox { Title = title, Content = txt, ButtonLeftName = btnLeft, ButtonRightName = btnRight };

        var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
        messageBox.Owner = mainWindow;

        messageBox.ButtonLeftClick += (_, e) =>
        {
            leftAction();
            messageBox.Close();
        };

        messageBox.ButtonRightClick += (_, _) =>
        {
            rightAction();
            messageBox.Close();
        };

        return messageBox.ShowDialog();
    }
}