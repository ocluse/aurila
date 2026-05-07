using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Aurila.Components.Layout;

public class AuHorizontalPager : AuHorizontalPagerBase<AuHorizontalPager>
{
    [Parameter]
    [EditorRequired]
    public int PageCount { get; set; }

    [Parameter]
    [EditorRequired]
    public RenderFragment<int> PageContent { get; set; } = null!;

    protected override int GetPageCount() => PageCount;

    protected override void BuildPageContent(int index, RenderTreeBuilder builder)
    {
        builder.AddContent(0, PageContent, index);
    }
}
