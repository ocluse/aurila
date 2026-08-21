using Aurila.Components.Controls;
using Aurila.Components.Input;
using Aurila.Components.Layout;
using Aurila.Components.Modals;
using Aurila.Design.Appearance;
using Aurila.Enums.Input;

namespace Aurila.Material.Appearance;

/// <summary>
/// Supplies Material defaults. Most styling is carried by the stylesheet keyed on Aurila's own
/// <c>au-*</c> classes; registrations here exist only where a control needs a default variant chosen,
/// or a class the control does not emit itself.
/// </summary>
public sealed class MaterialAppearanceProvider : AppearanceProvider
{
    public MaterialAppearanceProvider()
    {
        RegisterAppearance(MaterialButton.Filled);
        RegisterAppearance(MaterialIconButton.Standard);
        RegisterAppearance(MaterialFab.Primary);
        RegisterAppearance(MaterialChip.Assist);
        RegisterAppearance(MaterialCard.Elevated);
        RegisterAppearance(MaterialTopAppBar.Small);
        RegisterAppearance(MaterialDialog.Basic);
        RegisterAppearance(MaterialBottomSheet.Standard);

        RegisterAppearance(MaterialField.Outlined);
        RegisterAppearance(MaterialField.OutlinedBox);
        RegisterAppearance(MaterialField.OutlinedFor<AuDatePicker>());
        RegisterAppearance(MaterialField.OutlinedFor<AuTimePicker>());
        RegisterAppearance(MaterialField.OutlinedFor<AuDateTimePicker>());

        // AuGroupBox renders __header and __content but no class on its own root.
        RegisterAppearance(new ClassAppearance<AuGroupBox>("au-group-box", "md-group-box"));
    }

    public override IIconPainter IconPainter { get; } = new MaterialIconPainter();

    public override FieldHeaderStyle HeaderStyle => FieldHeaderStyle.Floating;
}
