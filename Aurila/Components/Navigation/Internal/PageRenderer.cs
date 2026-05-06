using Aurila.Contracts.Navigation;
using Aurila.Design;
using Aurila.Enums.Navigation;
using Aurila.Models.Navigation;

namespace Aurila.Components.Navigation.Internal;

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
            builder.AddAttribute(1, "class", GetClass());
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

    private string GetClass()
    {
        string stateClass = Entry.State switch
        {
            PageState.NavigatingTo => "au-page--entering",
            PageState.NavigatingFrom => "au-page--exiting",
            PageState.NavigatedTo => "au-page--active",
            _ => "au-page--inactive"
        };

        string navigationTypeClass = Entry.NavigationType switch
        {
            NavigationType.Push => "au-page--navigation-push",
            NavigationType.Pop => "au-page--navigation-pop",
            _ => string.Empty
        };

        return new ClassBuilder()
            .Add("au-page")
            .Add(stateClass)
            .Add(navigationTypeClass)
            .ToString();
    }

    public void Dispose()
    {
        Entry.Instance = null;
    }
}

