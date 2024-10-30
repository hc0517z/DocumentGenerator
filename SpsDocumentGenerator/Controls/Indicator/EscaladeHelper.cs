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
internal class EscaladeHelper
{
    /// <summary>
    ///     스트로크 대시 어레이 값 의존 속성
    /// </summary>
    public static readonly DependencyProperty StrokeDashArrayValueProperty =
        DependencyProperty.RegisterAttached("StrokeDashArrayValue",
            typeof(double),
            typeof(EscaladeHelper),
            new PropertyMetadata(0.0, OnStrokeDashArrayValueChanged));

    /// <summary>
    ///     스트로크 대시 어레이 값 속성 얻기 메서드
    /// </summary>
    /// <remarks>
    ///     스트로크 대시 어레이 값을 얻습니다.
    /// </remarks>
    /// <param name="path">
    ///     Path 인스턴스
    /// </param>
    /// <returns>
    ///     스트로크 대시 어레이 값
    /// </returns>
    public static double GetStrokeDashArrayValue(Path path)
    {
        return (double)path.GetValue(StrokeDashArrayValueProperty);
    }

    /// <summary>
    ///     스트로크 대시 어레이 값 속성 설정 메서드
    /// </summary>
    /// <remarks>
    ///     스트로크 대시 어레이 값을 설정합니다.
    /// </remarks>
    /// <param name="path">
    ///     Path 인스턴스
    /// </param>
    /// <param name="value">
    ///     스트로크 대시 어레이 값
    /// </param>
    public static void SetStrokeDashArrayValue(Path path, double value)
    {
        path.SetValue(StrokeDashArrayValueProperty, value);
    }

    /// <summary>
    ///     스트로크 대시 어레이 값 변경 이벤트 핸들러 메서드
    /// </summary>
    /// <remarks>
    ///     스트로크 대시 어레이 값을 변경합니다.
    /// </remarks>
    /// <param name="d">
    ///     Path 인스턴스
    /// </param>
    /// <param name="e">
    ///     이벤트 인자
    /// </param>
    private static void OnStrokeDashArrayValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Path path)
        {
            var value = (double)e.NewValue;
            path.StrokeDashArray = new DoubleCollection { value, 10 };
        }
    }
}