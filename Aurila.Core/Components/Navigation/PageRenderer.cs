using Aurila.Contracts.Navigation;
using Microsoft.AspNetCore.Components.Rendering;

namespace Aurila.Components.Navigation;

internal sealed class PageRenderer : ComponentBase, IDisposable
{
    [Parameter]
    [EditorRequired]
    public PageEntry Entry { get; set; } = null!;

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (Entry.ShouldRender)
        {
            //open a div element:
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", $"page {GetStateClass()} {GetNavigationTypeClass()}");
            builder.OpenComponent(2, Entry.PageType);

            //add ref:
            builder.AddComponentReferenceCapture(3, item =>
            {
                Entry.Instance = (IPage)item;
            });
            builder.CloseComponent();
            builder.CloseElement();
        }
    }

    private string GetStateClass()
    {
        return Entry.State switch
        {
            PageState.NavigatingTo => "entering",
            PageState.NavigatingFrom => "exiting",
            PageState.NavigatedTo => "active",
            _ => "inactive"
        };
    }

    private string GetNavigationTypeClass()
    {
        return Entry.NavigationType switch
        {
            NavigationType.Push => "navigation-push",
            NavigationType.Pop => "navigation-pop",
            _ => string.Empty
        };
    }

    public void Dispose()
    {
        Entry.Instance = null;
    }
}

