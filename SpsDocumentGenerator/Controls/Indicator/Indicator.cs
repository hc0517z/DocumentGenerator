using System.Windows;
using System.Windows.Controls;

namespace SpsDocumentGenerator.Controls.Indicator;

/// <summary>
///     Indicator 클래스
/// </summary>
/// <remarks>
///     공통
/// </remarks>
public class Indicator : Control
{
    /// <summary>
    ///     IndicatorType 의존 속성
    /// </summary>
    public static readonly DependencyProperty IndicatorTypeProperty =
        DependencyProperty.Register(nameof(IndicatorType),
            typeof(IndicatorType),
            typeof(Indicator),
            new PropertyMetadata(IndicatorType.Twist));

    static Indicator()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Indicator),
            new FrameworkPropertyMetadata(typeof(Indicator)));
    }

    /// <summary>
    ///     IndicatorType 속성
    /// </summary>
    public IndicatorType IndicatorType
    {
        get => (IndicatorType)GetValue(IndicatorTypeProperty);
        set => SetValue(IndicatorTypeProperty, value);
    }

    /// <summary>
    ///     템플릿 적용 메서드
    /// </summary>
    /// <remarks>
    ///     UpdateVisualState 메서드를 호출하여 시각 상태를 업데이트합니다.
    /// </remarks>
    public override void OnApplyTemplate()
    {
        UpdateVisualState();
    }

    /// <summary>
    ///     의존 속성 변경 이벤트 핸들러 메서드
    /// </summary>
    /// <remarks>
    ///     IsVisibleProperty와 IsEnabledProperty의 변경 이벤트에 대해서만 UpdateVisualState 메서드를 호출합니다.
    /// </remarks>
    /// <param name="e">
    ///     의존 속성 변경 이벤트 인자
    /// </param>
    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == IsVisibleProperty || e.Property == IsEnabledProperty) UpdateVisualState();
    }

    /// <summary>
    ///     시각 상태를 가져오는 메서드
    /// </summary>
    /// <remarks>
    ///     IsVisible과 IsEnabled 속성이 true인 경우 Active, 그렇지 않은 경우 Inactive를 반환합니다.
    /// </remarks>
    /// <returns>
    ///     시각 상태 문자열 값
    /// </returns>
    private string GetTargetState()
    {
        // IsVisible과 IsEnabled를 모두 고려해야 한다.
        return IsVisible && IsEnabled ? "Active" : "Inactive";
    }

    /// <summary>
    ///     시각 상태를 업데이트하는 메서드
    /// </summary>
    /// <remarks>
    ///     1. 템플릿이 적용되지 않은 경우 메서드를 종료합니다.
    ///     2. MainGrid를 가져옵니다.
    ///     3. MainGrid가 null인 경우 메서드를 종료합니다.
    ///     4. GetTargetState 메서드를 호출하여 시각 상태를 가져옵니다.
    ///     5. VisualStateManager를 사용하여 시각 상태를 업데이트합니다.
    /// </remarks>
    private void UpdateVisualState()
    {
        if (Template == null) return;

        var mainGrid = (FrameworkElement)GetTemplateChild("MainGrid");
        if (mainGrid == null) return;

        var targetState = GetTargetState();
        VisualStateManager.GoToElementState(mainGrid, targetState, true);
    }
}