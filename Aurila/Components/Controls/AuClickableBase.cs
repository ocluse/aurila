using Aurila.Components.Input;
using Aurila.Contracts.Navigation;
using Aurila.Design;
using Aurila.Models.Navigation;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Aurila.Components.Controls;

/// <summary>
/// Base for anything clickable.
/// </summary>
/// <remarks>
/// <para>
/// A clickable renders as an anchor when it has a destination and as a button when it does not. That
/// is not cosmetic: an anchor gives the user a real address to middle-click, open in a new tab, copy,
/// or have announced as a link. A button that navigates offers none of those, and an anchor that does
/// not navigate misleads everyone.
/// </para>
/// <para>
/// A disabled clickable always renders as a button, because there is no honest disabled anchor.
/// </para>
/// </remarks>
public abstract class AuClickableBase<TControl> : AuFormControlBase<TControl>, IFocusable
    where TControl : AuClickableBase<TControl>
{
    private ElementReference _element;

    [Inject]
    protected IJSRuntime JSRuntime { get; set; } = default!;

    [CascadingParameter]
    protected INavigator? Navigator { get; set; }

    /// <summary>
    /// Where this control navigates. When set, the control renders as an anchor.
    /// </summary>
    [Parameter]
    public NavTarget To { get; set; }

    /// <summary>
    /// Whether navigating replaces the current history entry instead of adding one.
    /// </summary>
    [Parameter]
    public bool Replace { get; set; }

    /// <summary>
    /// An ephemeral payload handed to the destination page. It is absent when the user later
    /// traverses back to the entry, so it may only save the page work, never determine what it shows.
    /// </summary>
    [Parameter]
    public object? Data { get; set; }

    /// <summary>
    /// Produces <see cref="Data"/> at click time, for payloads that are expensive or that change.
    /// </summary>
    [Parameter]
    public Func<object?>? GetData { get; set; }

    [Parameter]
    public EventCallback<MouseEventArgs> Clicked { get; set; }

    [Parameter]
    public bool StopPropagation { get; set; }

    /// <summary>
    /// Whether this control submits the enclosing form.
    /// </summary>
    [Parameter]
    public bool Submit { get; set; }

    /// <summary>
    /// The anchor's target. Only meaningful when the control renders as a link.
    /// </summary>
    [Parameter]
    public string? Target { get; set; }

    protected bool HasDestination => !To.IsEmpty;

    protected bool RendersAsLink => !Disabled && ResolvedUrl is not null;

    /// <summary>
    /// The destination's URL, resolved once per parameter change rather than per read.
    /// </summary>
    protected string? ResolvedUrl { get; private set; }

    /// <summary>
    /// Whether the destination leaves the app, in which case the browser handles the click and the
    /// navigator must not also act on it.
    /// </summary>
    private bool LeavesApp => ResolvedUrl is { } url
        && (url.StartsWith("//", StringComparison.Ordinal)
            || (Uri.TryCreate(url, UriKind.Absolute, out var absolute) && absolute.IsAbsoluteUri));

    private bool IsDownload => AdditionalAttributes?.ContainsKey("download") == true;

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (Submit && HasDestination)
        {
            throw new InvalidOperationException(
                $"{GetType().Name} has both Submit and a destination. A control either submits a form " +
                "or navigates; it cannot do both.");
        }

        ResolvedUrl = HasDestination
            && Navigator is not null
            && Navigator.TryGetUrl(To, out string url) ? url : null;
    }

    public virtual async Task FocusAsync()
    {
        if (FocusElement.HasValue)
        {
            await FocusElement.Value.FocusAsync();
        }
    }

    public virtual async Task BlurAsync()
    {
        if (FocusElement.HasValue)
        {
            await JSRuntime.InvokeVoidAsync("HTMLElement.prototype.blur.call", FocusElement.Value);
        }
    }

    protected virtual void BuildControlClass(ClassBuilder builder) { }

    protected virtual async Task OnClickedAsync(MouseEventArgs e)
    {
        if (Clicked.HasDelegate)
        {
            await Clicked.InvokeAsync(e);
        }

        if (!HasDestination || Navigator is null || !ClaimsClick(e))
        {
            return;
        }

        object? data = GetData is not null ? GetData() : Data;

        if (Replace)
        {
            Navigator.Replace(To, data);
        }
        else
        {
            Navigator.Navigate(To, data);
        }
    }

    /// <summary>
    /// Whether this click is the app's to handle.
    /// </summary>
    /// <remarks>
    /// Must agree with the ledger's capture-phase handler, which decides whether to suppress the
    /// browser's own navigation. If the two disagree, a click either navigates twice or not at all.
    /// </remarks>
    private bool ClaimsClick(MouseEventArgs e)
        => !LeavesApp
            && !IsDownload
            && Target is null or "_self"
            && e.Button == 0
            && !e.CtrlKey
            && !e.MetaKey
            && !e.ShiftKey
            && !e.AltKey;

    protected override sealed void BuildClass(ClassBuilder builder)
    {
        base.BuildClass(builder);
        BuildControlClass(builder);

        builder.Add("au-clickable");
        builder.AddIf(RendersAsLink, "au-clickable--link");
        builder.AddIf(Disabled, "au-clickable--disabled");
    }

    protected override void BuildAttributes(IDictionary<string, object> attributes)
    {
        base.BuildAttributes(attributes);

        if (!RendersAsLink)
        {
            attributes["role"] = "button";
        }
    }

    /// <summary>
    /// Drops attributes that do not belong on the element actually chosen, so that a supplied
    /// <c>href</c> cannot end up on a button or a <c>type</c> on an anchor.
    /// </summary>
    private Dictionary<string, object> GetAppliedAttributes(bool asButton)
    {
        var attributes = GetAppliedAttributes();

        foreach (var name in asButton ? LinkOnlyAttributes : ButtonOnlyAttributes)
        {
            attributes.Remove(name);
        }

        return attributes;
    }

    private static readonly string[] LinkOnlyAttributes = ["href", "download", "target", "rel", "hreflang"];

    private static readonly string[] ButtonOnlyAttributes = ["type", "disabled", "form", "formaction"];

    protected virtual ElementReference? FocusElement => _element;

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        string? href = RendersAsLink ? ResolvedUrl : null;

        builder.OpenElement(0, href is null ? "button" : "a");
        {
            builder.AddMultipleAttributes(1, GetAppliedAttributes(href is null));

            if (href is null)
            {
                builder.AddAttribute(2, "type", Submit ? "submit" : "button");

                if (Disabled)
                {
                    builder.AddAttribute(3, "disabled", true);
                }
            }
            else
            {
                builder.AddAttribute(4, "href", href);

                if (!LeavesApp)
                {
                    builder.AddAttribute(5, "data-au-link", "");
                }

                if (Target is not null)
                {
                    builder.AddAttribute(6, "target", Target);
                }
            }

            if (!Disabled)
            {
                builder.AddAttribute(7, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, OnClickedAsync));
            }

            if (StopPropagation)
            {
                builder.AddEventStopPropagationAttribute(8, "onclick", true);
            }

            builder.OpenRegion(9);
            {
                BuildContent(builder);
            }
            builder.CloseRegion();

            builder.AddElementReferenceCapture(10, reference => _element = reference);
        }
        builder.CloseElement();
    }

    protected abstract void BuildContent(RenderTreeBuilder builder);
}
