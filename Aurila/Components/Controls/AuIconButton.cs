using Aurila.Design;

namespace Aurila.Components.Controls;

public class AuIconButton : AuButtonBase<AuIconButton>
{
    protected override void BuildControlClass(ClassBuilder builder)
    {
        base.BuildControlClass(builder);
        builder.Add("au-icon-button");
    }
}
