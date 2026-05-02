using Aurila.Design;

namespace Aurila.Components.Modals;
public class Dialog : ModalBase<Dialog>
{
    protected override void BuildClass(ClassBuilder builder)
    {
        base.BuildClass(builder);
        builder.Add("au-dialog");
    }
}
