using Aurila.Contracts.Navigation;
using Aurila.Design;
using Aurila.Enums.Navigation;
using Aurila.Models.Navigation;
using Aurila.Services.Navigation;

namespace Aurila.Components.Navigation.Internal;

internal sealed class PageRenderer(PageParametersCache pageParametersCache) : ComponentBase, IDisposable
{
    [Parameter]
    [EditorRequired]
    public PageEntry Entry { get; set; } = null!;

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var pageParameters = pageParametersCache.GetAvailableParameters(
            Entry.PageType,
            Entry.RouteParameters,
            Entry.RouteArgument,
            Entry.EntryState,
            Entry.MemoryState);

        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", GetClass());
        builder.OpenComponent(2, Entry.PageType);

        if (pageParameters.Count > 0)
        {
            builder.AddMultipleAttributes(3, pageParameters!);
        }

        builder.AddAttribute(4, nameof(PageBindingContext), Entry.Binding);
        builder.AddComponentReferenceCapture(5, item => Entry.Instance = (IPage)item);

        builder.CloseComponent();
        builder.CloseElement();
    }

    private string GetClass()
    {
        string stateClass = Entry.State switch
        {
            PageState.NavigatingTo => "au-page--entering",
            PageState.NavigatingFrom => "au-page--exiting",
            PageState.NavigatedTo => "au-page--active",
            _ => "au-page--inactive"
        };

        string intentClass = Entry.Intent switch
        {
            NavIntent.Push or NavIntent.Forward => "au-page--navigation-push",
            NavIntent.Back or NavIntent.Jump => "au-page--navigation-pop",
            _ => string.Empty
        };

        return new ClassBuilder()
            .Add("au-page")
            .Add(stateClass)
            .Add(intentClass)
            .ToString();
    }

    public void Dispose()
    {
        Entry.Instance = null;
    }
}
