using Aurila.Contracts.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurila.Models;
internal class PageEntry
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public required Type PageType { get; init; }

    public required object? Data { get; set; }

    public required bool IsSingleton { get; init; }

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

    public static PageEntry Create(Type pageType, object? data = null)
    {
        return new PageEntry
        {
            PageType = pageType,
            Data = data,
            IsSingleton = typeof(ISingletonPage).IsAssignableFrom(pageType),
            State = PageState.None,
            NavigationType = null
        };
    }
}

