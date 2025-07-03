using Aurila.Contracts.Components;
using Ocluse.LiquidSnow.Data;
using System.Collections.Specialized;

namespace Aurila.Components.Controls;

public sealed class LazyList<TKey, TItem> : ComponentBase, IDisposable where TKey : notnull
{
    [Parameter]
    [EditorRequired]
    public Pager<TKey, TItem> Pager { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public RenderFragment<TItem> ItemTemplate { get; set; } = null!;

    [Parameter]
    public RenderFragment? EmptyTemplate { get; set; }

    [Parameter]
    public RenderFragment? ErrorTemplate { get; set; }

    [Parameter]
    public RenderFragment<int>? PlaceholderTemplate { get; set; }

    [Parameter]
    public bool InvertedScroll { get; set; }


    [CascadingParameter]
    public IScrollController? ScrollController { get; set; }

    protected override void OnInitialized()
    {
        Pager.CollectionChanged += OnPagerDataChanged;
        Pager.StateChanged += PagerStateChanged;
        if (ScrollController != null)
        {
            ScrollController.ScrollChanged += OnScrollChanged;
        }
    }

    public void Dispose()
    {
        Pager.CollectionChanged -= OnPagerDataChanged;

        if (ScrollController != null)
        {
            ScrollController.ScrollChanged -= OnScrollChanged;
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (Pager.State.Prepend == LoadState.Loading)
        {
            builder.OpenRegion(1);
            RenderPlaceholders(builder);
            builder.CloseRegion();
        }

        if (Pager.State.Refresh == LoadState.Loading)
        {
            builder.OpenRegion(2);
            RenderPlaceholders(builder);
            builder.CloseRegion();
        }
        else if (Pager.State.Refresh == LoadState.NotLoading)
        {
            if (Pager.Items.Count == 0)
            {
                if (EmptyTemplate != null)
                {
                    builder.AddContent(3, EmptyTemplate);
                }
                else
                {
                    builder.AddContent(4, "No items found.");
                }
            }
            else
            {
                builder.OpenRegion(5);
                RenderItems(builder);
                builder.CloseRegion();
            }
        }
        else if (Pager.State.Refresh == LoadState.Error)
        {
            if (ErrorTemplate != null)
            {
                builder.AddContent(6, ErrorTemplate);
            }
            else
            {
                builder.AddContent(7, "An error occurred.");
            }
        }

        if (Pager.State.Append == LoadState.Loading)
        {
            builder.OpenRegion(8);
            RenderPlaceholders(builder);
            builder.CloseRegion();
        }
    }

    private void RenderItems(RenderTreeBuilder builder)
    {
        foreach (var item in Pager.Items)
        {
            builder.AddContent(1, ItemTemplate, item);
        }
    }

    private void RenderPlaceholders(RenderTreeBuilder builder)
    {
        if (PlaceholderTemplate != null)
        {
            for (int i = 0; i < Pager.PageSize; i++)
            {
                builder.AddContent(1, PlaceholderTemplate, i);
            }
        }
        else
        {
            builder.AddContent(2, "Loading...");
        }
    }

    private void PagerStateChanged(object? sender, PagerStateChangedArgs e)
    {
        _ = InvokeAsync(StateHasChanged);
    }

    private void OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (e.ScrollPosition == 0)
        {
            if (InvertedScroll)
                Pager.ReachedEnd();
            else
                Pager.ReachedStart();
        }
        else if (e.Progress > 0.99)
        {
            if (InvertedScroll)
                Pager.ReachedStart();
            else
                Pager.ReachedEnd();
        }
    }

    private void OnPagerDataChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        _ = InvokeAsync(StateHasChanged);
    }
}

