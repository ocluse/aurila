namespace Aurila.Services;

/// <summary>
/// Broker between modal components and the ModalHost. Modals register a RenderFragment
/// here when they open; ModalHost renders all registered fragments at the root of the app,
/// keeping them outside any ancestor stacking/containing-block context.
/// </summary>
public class ModalHostService
{
    private readonly List<ModalRegistration> _registrations = [];

    public IReadOnlyList<ModalRegistration> Registrations => _registrations;

    /// <summary>
    /// Raised whenever the registration list changes so ModalHost knows to re-render.
    /// </summary>
    public event Action? OnChanged;

    internal ModalRegistration Register(RenderFragment fragment)
    {
        var registration = new ModalRegistration(fragment);
        _registrations.Add(registration);
        OnChanged?.Invoke();
        return registration;
    }

    internal void Unregister(ModalRegistration registration)
    {
        _registrations.Remove(registration);
        OnChanged?.Invoke();
    }

    /// <summary>
    /// Called by a modal to notify the host that it should re-render all fragments —
    /// for example, when closing animation state changes without touching the registration list.
    /// </summary>
    internal void NotifyChanged() => OnChanged?.Invoke();
}

/// <summary>
/// Represents a single modal's render fragment registered with ModalHostService.
/// Held by ModalBase and passed back to Unregister when the modal closes.
/// </summary>
public sealed class ModalRegistration
{
    internal ModalRegistration(RenderFragment fragment) => Fragment = fragment;
    public RenderFragment Fragment { get; }
}
