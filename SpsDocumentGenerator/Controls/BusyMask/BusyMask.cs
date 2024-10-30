using System.Windows;
using System.Windows.Controls;
using SpsDocumentGenerator.Base;
using SpsDocumentGenerator.Controls.Indicator;

namespace SpsDocumentGenerator.Controls.BusyMask;

/// <summary>
///     대기 컨트롤 클래스
/// </summary>
/// <remarks>
///     공통
/// </remarks>
[TemplateVisualState(Name = VisualStates.StateHidden, GroupName = VisualStates.GroupVisibility)]
[TemplateVisualState(Name = VisualStates.StateVisible, GroupName = VisualStates.GroupVisibility)]
public class BusyMask : ContentControl
{
    /// <summary>
    ///     대기 상태 의존 속성
    /// </summary>
    public static readonly DependencyProperty IsBusyProperty =
        DependencyProperty.Register(nameof(IsBusy),
            typeof(bool),
            typeof(BusyMask),
            new PropertyMetadata(false, OnIsBusyChanged));

    /// <summary>
    ///     대기 메시지 의존 속성
    /// </summary>
    public static readonly DependencyProperty BusyContentProperty =
        DependencyProperty.Register(nameof(BusyContent),
            typeof(string),
            typeof(BusyMask),
            new PropertyMetadata("Please wait..."));

    /// <summary>
    ///     대기 인디케이터 타입 의존 속성
    /// </summary>
    public static readonly DependencyProperty IndicatorTypeProperty =
        DependencyProperty.Register(nameof(IndicatorType),
            typeof(IndicatorType),
            typeof(BusyMask),
            new PropertyMetadata(IndicatorType.Twist));

    /// <summary>
    ///     대기 후 포커스 의존 속성
    /// </summary>
    public static readonly DependencyProperty FocusAfterBusyProperty =
        DependencyProperty.Register(nameof(FocusAfterBusy),
            typeof(Control),
            typeof(BusyMask),
            new PropertyMetadata(null));

    /// <summary>
    ///     대기 컨트롤 클래스 생성자
    /// </summary>
    /// <remarks>
    ///     기본 스타일 키를 설정합니다.
    /// </remarks>
    static BusyMask()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(BusyMask),
            new FrameworkPropertyMetadata(typeof(BusyMask)));
    }

    /// <summary>
    ///     대기 상태 속성
    /// </summary>
    public bool IsBusy
    {
        get => (bool)GetValue(IsBusyProperty);
        set => SetValue(IsBusyProperty, value);
    }

    /// <summary>
    ///     대기 메시지 속성
    /// </summary>
    public string BusyContent
    {
        get => (string)GetValue(BusyContentProperty);
        set => SetValue(BusyContentProperty, value);
    }

    /// <summary>
    ///     대기 인디케이터 타입 속성
    /// </summary>
    public IndicatorType IndicatorType
    {
        get => (IndicatorType)GetValue(IndicatorTypeProperty);
        set => SetValue(IndicatorTypeProperty, value);
    }

    /// <summary>
    ///     대기 후 포커스 속성
    /// </summary>
    public Control FocusAfterBusy
    {
        get => (Control)GetValue(FocusAfterBusyProperty);
        set => SetValue(FocusAfterBusyProperty, value);
    }

    /// <summary>
    ///     대기 상태 변경 이벤트 (외부)
    /// </summary>
    /// <remarks>
    ///     내부 OnIsBusyChanged 메서드를 호출합니다.
    /// </remarks>
    /// <param name="d">
    ///     대기 컨트롤 인스턴스
    /// </param>
    /// <param name="e">
    ///     의존 속성 변경 이벤트 인스턴스
    /// </param>
    private static void OnIsBusyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((BusyMask)d).OnIsBusyChanged(e);
    }

    /// <summary>
    ///     대기 상태 변경 이벤트 (내부)
    /// </summary>
    /// <remarks>
    ///     1. 대기 상태가 변경되면 포커스를 설정합니다.
    ///     2. ChangeVisualState 메서드를 호출합니다.
    /// </remarks>
    /// <param name="e">
    ///     의존 속성 변경 이벤트 인스턴스
    /// </param>
    protected virtual void OnIsBusyChanged(DependencyPropertyChangedEventArgs e)
    {
        if (!(bool)e.NewValue)
            if (FocusAfterBusy != null)
                FocusAfterBusy.Dispatcher.Delay(100, _ => { FocusAfterBusy.Focus(); });

        ChangeVisualState((bool)e.NewValue);
    }

    /// <summary>
    ///     템플릿 적용 이벤트 (오버라이드)
    /// </summary>
    /// <remarks>
    ///     ChangeVisualState 메서드를 호출합니다.
    /// </remarks>
    public override void OnApplyTemplate()
    {
        ChangeVisualState();
    }

    /// <summary>
    ///     시각 상태 변경 메서드
    /// </summary>
    /// <remarks>
    ///     1. 대기 상태에 따라 시각 상태를 변경합니다.
    ///     2. VisualStateManager.GoToState 메서드를 호출합니다.
    /// </remarks>
    /// <param name="isBusyContentVisible">
    ///     대기 상태 (기본값: false)
    /// </param>
    protected virtual void ChangeVisualState(bool isBusyContentVisible = false)
    {
        VisualStateManager.GoToState(this, isBusyContentVisible ? "Visible" : "Hidden", true);
    }
}