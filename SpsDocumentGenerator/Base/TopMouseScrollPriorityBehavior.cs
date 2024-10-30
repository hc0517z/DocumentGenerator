using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SpsDocumentGenerator.Base;

public class TopMouseScrollPriorityBehavior
{
    public static readonly DependencyProperty TopMouseScrollPriorityProperty =
        DependencyProperty.RegisterAttached("TopMouseScrollPriority", typeof(bool), typeof(TopMouseScrollPriorityBehavior),
            new PropertyMetadata(false, OnPropertyChanged));

    public static bool GetTopMouseScrollPriority(ScrollViewer obj)
    {
        return (bool)obj.GetValue(TopMouseScrollPriorityProperty);
    }

    public static void SetTopMouseScrollPriority(ScrollViewer obj, bool value)
    {
        obj.SetValue(TopMouseScrollPriorityProperty, value);
    }

    private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var scrollViewer = d as ScrollViewer;
        if (scrollViewer == null)
            throw new InvalidOperationException(
                $"{nameof(TopMouseScrollPriorityBehavior)}.{nameof(TopMouseScrollPriorityProperty)} can only be applied to controls of type {nameof(ScrollViewer)}");
        if (e.NewValue == e.OldValue)
            return;
        if ((bool)e.NewValue)
            scrollViewer.PreviewMouseWheel += ScrollViewer_PreviewMouseWheel;
        else
            scrollViewer.PreviewMouseWheel -= ScrollViewer_PreviewMouseWheel;
    }

    private static void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        var scrollViewer = (ScrollViewer)sender;
        scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta);
        e.Handled = true;
    }
}