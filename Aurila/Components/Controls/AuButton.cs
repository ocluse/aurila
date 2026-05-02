using Aurila.Design;

namespace Aurila.Components.Controls;

public class AuButton : AuButtonBase<AuButton>
{
    protected override void BuildControlClass(ClassBuilder builder)
    {
        base.BuildControlClass(builder);
        builder.Add("au-button");
    }
}
