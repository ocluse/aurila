using Aurila.Design;
using Aurila.Contracts.Navigation;
using Aurila.Models.Navigation;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Aurila.Components.Controls;

/// <summary>
/// Shared interaction policy for controls that can invoke an action or navigate to a destination.
/// Rendering is left to derived types so each control can choose valid native or flow-content
/// semantics.
/// </summary>
public abstract class AuInteractionBase<TControl> : AuControlBase<TControl>, IFocusable
    where TControl : AuInteractionBase<TControl>
{
    private ElementReference _element;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = null!;

    [CascadingParameter]
    protected INavigator? Navigator { get; set; }

    [Parameter]
    public NavTarget To { get; set; }

    [Parameter]
    public bool Replace { get; set; }

    [Parameter]
    public object? State { get; set; }

    [Parameter]
    public Func<object?>? GetState { get; set; }

    [Parameter]
    public EventCallback<MouseEventArgs> Clicked { get; set; }

    [Parameter]
    public bool StopPropagation { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public string? Target { get; set; }

    protected bool HasDestination => !To.IsEmpty;

    /// <summary>
    /// Whether this instance currently exposes interaction. Native clickables override the default
    /// by remaining interactive even when no callback or destination has been assigned.
    /// </summary>
    protected virtual bool IsInteractive => HasDestination || Clicked.HasDelegate;

    protected bool RendersAsLink => IsInteractive && !Disabled && ResolvedUrl is not null;

    protected string? ResolvedUrl { get; private set; }

    protected ElementReference InteractionElement => _element;

    protected bool LeavesApp => ResolvedUrl is { } url
        && (url.StartsWith("//", StringComparison.Ordinal)
            || (Uri.TryCreate(url, UriKind.Absolute, out var absolute) && absolute.IsAbsoluteUri));

    private bool IsDownload => AdditionalAttributes?.ContainsKey("download") == true;

    private string? EffectiveTarget
        => Target ?? (AdditionalAttributes?.TryGetValue("target", out object? target) == true
            ? target?.ToString()
            : null);

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        ResolvedUrl = HasDestination
            && Navigator is not null
            && Navigator.TryGetUrl(To, out string url) ? url : null;
    }

    protected override void BuildClass(ClassBuilder builder)
    {
        base.BuildClass(builder);
        builder.AddIf(IsInteractive, "au-clickable");
        builder.AddIf(RendersAsLink, "au-clickable--link");
        builder.AddIf(IsInteractive && Disabled, "au-clickable--disabled");
    }

    protected override void BuildStyle(StyleBuilder builder)
    {
        base.BuildStyle(builder);

        if (IsInteractive)
        {
            builder.Add("cursor", Disabled ? "default" : "pointer");
        }
    }

    protected Dictionary<string, object> GetInteractionAttributes(InteractionElementKind elementKind)
    {
        var attributes = GetAppliedAttributes();

        if (elementKind != InteractionElementKind.Anchor)
        {
            RemoveAttributes(attributes, LinkOnlyAttributes);
        }

        if (elementKind != InteractionElementKind.Button)
        {
            RemoveAttributes(attributes, ButtonOnlyAttributes);
        }

        return attributes;
    }

    protected void AddLinkAttributes(RenderTreeBuilder builder, int sequence)
    {
        builder.AddAttribute(sequence, "href", ResolvedUrl);

        if (!LeavesApp)
        {
            builder.AddAttribute(sequence + 1, "data-au-link", string.Empty);
        }

        if (Target is not null)
        {
            builder.AddAttribute(sequence + 2, "target", Target);
        }
    }

    protected void AddClickAttributes(RenderTreeBuilder builder, int sequence)
    {
        if (!Disabled)
        {
            builder.AddAttribute(
                sequence,
                "onclick",
                EventCallback.Factory.Create<MouseEventArgs>(this, OnClickedAsync));
        }

        if (StopPropagation)
        {
            builder.AddEventStopPropagationAttribute(sequence + 1, "onclick", true);
        }
    }

    protected void CaptureInteractionElement(ElementReference element)
    {
        _element = element;
    }

    protected async Task OnClickedAsync(MouseEventArgs e)
    {
        if (Clicked.HasDelegate)
        {
            await Clicked.InvokeAsync(e);
        }

        if (!HasDestination || Navigator is null || !ClaimsClick(e))
        {
            return;
        }

        object? state = GetState is not null ? GetState() : State;

        if (Replace)
        {
            Navigator.Replace(To, state);
        }
        else
        {
            Navigator.Navigate(To, state);
        }
    }

    private bool ClaimsClick(MouseEventArgs e)
        => !LeavesApp
            && !IsDownload
            && EffectiveTarget is null or "_self"
            && e.Button == 0
            && !e.CtrlKey
            && !e.MetaKey
            && !e.ShiftKey
            && !e.AltKey;

    public async Task FocusAsync()
    {
        await _element.FocusAsync();
    }

    public async Task BlurAsync()
    {
        await JSRuntime.InvokeVoidAsync("HTMLElement.prototype.blur.call", _element);
    }

    private static void RemoveAttributes(IDictionary<string, object> attributes, IEnumerable<string> names)
    {
        foreach (string name in names)
        {
            attributes.Remove(name);
        }
    }

    private static readonly string[] LinkOnlyAttributes = ["href", "download", "target", "rel", "hreflang"];

    private static readonly string[] ButtonOnlyAttributes = ["type", "disabled", "form", "formaction"];

    protected enum InteractionElementKind
    {
        Container,
        Button,
        Anchor
    }
}
