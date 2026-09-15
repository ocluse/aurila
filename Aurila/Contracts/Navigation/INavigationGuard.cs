using Aurila.Models.Navigation;

namespace Aurila.Contracts.Navigation;

/// <summary>
/// Something that may refuse to let the user leave the current page.
/// </summary>
/// <remarks>
/// <para>
/// The two members are deliberately asymmetric. Whether a guard <em>might</em> object has to be known
/// synchronously, before a blockable navigation commits, so <see cref="IsArmed"/> must be cheap and
/// must not block. Only when it is true is the navigation held back and
/// <see cref="CanLeaveAsync"/> asked, which may take as long as it likes and may show UI. A traversal
/// started from browser UI is not blockable on the History API fallback; in that case the guard is
/// notified after commit with <see cref="NavigationLeaveContext.CanBlock"/> false.
/// </para>
/// <para>
/// A guard that is never armed costs nothing: navigations take the fast path and commit immediately.
/// </para>
/// </remarks>
public interface INavigationGuard
{
    bool IsArmed { get; }

    ValueTask<bool> CanLeaveAsync(NavigationLeaveContext context);
}
