using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurila.Components.Controls;

public class FloatingActionButton : ClickableBase<FloatingActionButton>
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override void BuildContent(RenderTreeBuilder builder)
    {
        builder.AddContent(0, ChildContent);
    }

    protected override void BuildControlClass(ClassBuilder builder)
    {
        base.BuildControlClass(builder);
        builder.Add("au-floating-action-button");
    }
}
