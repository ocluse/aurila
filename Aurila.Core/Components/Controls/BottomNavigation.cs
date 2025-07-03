using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurila.Components.Controls;

public class BottomNavigation : ControlBase<BottomNavigation>
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "nav");
        {
            builder.AddMultipleAttributes(2, GetAppliedAttributes());
            builder.AddContent(3, ChildContent);
        }
        builder.CloseElement();
    }

    protected override void BuildClass(ClassBuilder builder)
    {
        base.BuildClass(builder);
        builder.Add("au-bottom-navigation");
    }
}
