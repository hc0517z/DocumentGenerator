using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SpsDocumentGenerator.Controls.Indicator;

/// <summary>
///     EllipseHelper 클래스
/// </summary>
/// <remarks>
///     공통
/// </remarks>
internal class EllipseHelper
{
    /// <summary>
    ///     스트로크 대시 어레이 값 의존 속성
    /// </summary>
    public static readonly DependencyProperty StrokeDashArrayValueProperty =
        DependencyProperty.RegisterAttached("StrokeDashArrayValue",
            typeof(double),
            typeof(EllipseHelper),
            new PropertyMetadata(0.0, OnStrokeDashArrayValueChanged));

    /// <summary>
    ///     스트로크 대시 어레이 값 속성 얻기 메서드
    /// </summary>
    /// <remarks>
    ///     스트로크 대시 어레이 값을 얻습니다.
    /// </remarks>
    /// <param name="ellipse">
    ///     Ellipse 인스턴스
    /// </param>
    /// <returns>
    ///     스트로크 대시 어레이 값
    /// </returns>
    public static double GetStrokeDashArrayValue(Ellipse ellipse)
    {
        return (double)ellipse.GetValue(StrokeDashArrayValueProperty);
    }

    /// <summary>
    ///     스트로크 대시 어레이 값 속성 설정 메서드
    /// </summary>
    /// <remarks>
    ///     스트로크 대시 어레이 값을 설정합니다.
    /// </remarks>
    /// <param name="ellipse">
    ///     Ellipse 인스턴스
    /// </param>
    /// <param name="value">
    ///     스트로크 대시 어레이 값
    /// </param>
    public static void SetStrokeDashArrayValue(Ellipse ellipse, double value)
    {
        ellipse.SetValue(StrokeDashArrayValueProperty, value);
    }

    /// <summary>
    ///     스트로크 대시 어레이 값 변경 이벤트 핸들러 메서드
    /// </summary>
    /// <remarks>
    ///     1. Ellipse 인스턴스를 얻습니다.
    ///     2. 스트로크 대시 어레이 값을 얻습니다.
    ///     3. 스트로크 대시 어레이 값을 설정합니다.
    /// </remarks>
    /// <param name="d">
    ///     Ellipse 인스턴스
    /// </param>
    /// <param name="e">
    ///     이벤트 인자
    /// </param>
    private static void OnStrokeDashArrayValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Ellipse ellipse)
        {
            var value = (double)e.NewValue;
            ellipse.StrokeDashArray = new DoubleCollection { value, 100 };
        }
    }
}