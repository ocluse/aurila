using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Aurila.Components.Layout;

public class AuHorizontalListPager<TItem> : AuHorizontalPagerBase<AuHorizontalListPager<TItem>>
{
    [Parameter]
    [EditorRequired]
    public IReadOnlyList<TItem> Items { get; set; } = Array.Empty<TItem>();

    [Parameter]
    [EditorRequired]
    public RenderFragment<TItem> ItemTemplate { get; set; } = null!;

    protected override int GetPageCount() => Items?.Count ?? 0;

    protected override void BuildPageContent(int index, RenderTreeBuilder builder)
    {
        if (Items != null && index >= 0 && index < Items.Count)
        {
            builder.AddContent(0, ItemTemplate, Items[index]);
        }
    }
}
