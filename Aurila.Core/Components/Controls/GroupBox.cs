using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurila.Components.Controls;
public class GroupBox : ControlBase<GroupBox>
{
    [Parameter]
    public RenderFragment? Header { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        {
            builder.AddMultipleAttributes(1, GetAppliedAttributes());
            if(Header != null)
            {
                builder.OpenElement(2, "div");
                {
                    builder.AddAttribute(3, "class", "au-group-box__header");
                    builder.AddContent(4, Header);
                }
                builder.CloseElement();
            }
            
            builder.OpenElement(5, "div");
            {
                builder.AddAttribute(6, "class", "au-group-box__content");
                if (ChildContent != null)
                {
                    builder.AddContent(7, ChildContent);
                }
            }
            builder.CloseElement();

        }
        builder.CloseElement();
    }
}
