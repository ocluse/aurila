namespace Aurila.Components.Modals;
public class ModalBottomSheet : ModalBase<ModalBottomSheet>
{
    protected override void BuildClass(ClassBuilder builder)
    {
        builder.Add("au-modal-bottom-sheet");
    }
}
