namespace Aurila.Contracts.Navigation;

/// <summary>
/// Marks a component that can be shown by a navigation host.
/// </summary>
/// <remarks>
/// A page has no navigation lifecycle of its own. Everything it is given arrives as a declared
/// parameter, so it reads its inputs in <c>OnInitializedAsync</c> or <c>OnParametersSet</c> like any
/// other Blazor component, cleans up in <see cref="IDisposable"/>, and refuses to be left by
/// implementing <see cref="INavigationGuard"/>. <c>AuPage</c> is the supported base.
/// </remarks>
public interface IPage
{
}

/// <summary>
/// A page of which there is at most one history entry and one instance.
/// </summary>
/// <remarks>
/// Navigating to a singleton page that already exists in history travels to its entry rather than
/// stacking a second copy. Blazor will not run <c>OnInitialized</c> again on return; override
/// <c>AuPage.OnResumed</c> to be told.
/// </remarks>
public interface ISingletonPage : IPage
{
}
