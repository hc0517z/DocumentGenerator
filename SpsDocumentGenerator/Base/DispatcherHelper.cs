using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace SpsDocumentGenerator.Base;

/// <summary>
///     디스패처 관련 Helper 클래스
/// </summary>
/// <remarks>
///     공통
/// </remarks>
public static class DispatcherHelper
{
    /// <summary>
    ///     딜레이 후 Action 실행 메서드
    /// </summary>
    /// <remarks>
    ///     입력받은 delay 시간 후에 action을 실행합니다.
    /// </remarks>
    /// <param name="dispatcher">
    ///     Dispatcher 객체
    /// </param>
    /// <param name="delay">
    ///     딜레이 시간
    /// </param>
    /// <param name="action">
    ///     실행할 Action
    /// </param>
    /// <param name="param">
    ///     Action에 전달할 파라미터 (기본값: null)
    /// </param>
    public static void Delay(this Dispatcher dispatcher, int delay, Action<object> action, object param = null)
    {
        Task.Delay(delay).ContinueWith(_ => { dispatcher.Invoke(action, param); });
    }

    /// <summary>
    ///     비동기 Action 확인 및 실행 메서드
    /// </summary>
    /// <remarks>
    ///     1. action이 null이면 아무것도 하지 않습니다.
    ///     2. 디스패처에 접근 가능하면 action을 실행합니다.
    ///     3. 디스패처에 접근 불가능하면 BeginInvoke를 통해 action을 실행합니다.
    /// </remarks>
    /// <param name="action">
    ///     실행할 Action
    /// </param>
    public static void CheckBeginInvokeOnUI(Action action)
    {
        if (action == null)
            return;

        var dispatcher = Application.Current.Dispatcher;
        if (dispatcher.CheckAccess())
            action();
        else
            dispatcher.BeginInvoke(action);
    }
}