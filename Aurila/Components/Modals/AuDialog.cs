using Aurila.Design;

namespace Aurila.Components.Modals;
public class AuDialog : AuModalBase<AuDialog>
{
    protected override void BuildClass(ClassBuilder builder)
    {
        base.BuildClass(builder);
        builder.Add("au-dialog");
    }
}
