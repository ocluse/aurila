using Aurila.Contracts.Navigation;
using Aurila.Enums.Navigation;

namespace Aurila.Models.Navigation;
internal class PageEntry
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public bool IsRestored { get; set; }

    public required Type PageType { get; init; }

    public required object? Data { get; set; }

    public required bool IsSingleton { get; init; }

    public string? Route { get; set; }

    public IPage? Instance { get; set; }

    public IPage EnsuredInstance
    {
        get
        {
            if (Instance == null)
            {
                throw new InvalidOperationException($"Page instance from {Id} is not set.");
            }
            return Instance;
        }
    }

    public PageState State { get; set; } = PageState.None;

    public NavigationType? NavigationType { get; set; }

    public bool ShouldRender
    {
        get
        {
            if (State is PageState.None or PageState.NavigatedFrom)
            {
                return IsSingleton;
            }
            else
            {
                return true;
            }
        }
    }

    public static PageEntry Create(Type pageType, object? data, string? route)
    {
        return new PageEntry
        {
            PageType = pageType,
            Data = data,
            IsSingleton = typeof(ISingletonPage).IsAssignableFrom(pageType),
            State = PageState.None,
            NavigationType = null,
            Route = route
        };
    }
}

