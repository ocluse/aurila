using Aurila.Components.Controls;
using Aurila.Components.Input;
using Aurila.Components.Layout;
using Aurila.Design.Appearance;
using Aurila.Enums.Input;

namespace Aurila.Fluent.Appearance;

public sealed class FluentAppearanceProvider : AppearanceProvider
{
    public FluentAppearanceProvider()
    {
        RegisterAppearance(FluentButton.Secondary);
        RegisterAppearance(FluentIconButton.Subtle);
        RegisterAppearance(FluentFab.Brand);
        RegisterAppearance(FluentTag.Filled);
        RegisterAppearance(FluentCard.Filled);
        RegisterAppearance(FluentTopAppBar.Neutral);
        RegisterAppearance(FluentDialog.Standard);
        RegisterAppearance(FluentBottomSheet.Standard);
        RegisterAppearance(FluentField.Outline);
        RegisterAppearance(FluentField.OutlineBox);
        RegisterAppearance(FluentField.OutlineFor<AuDatePicker>());
        RegisterAppearance(FluentField.OutlineFor<AuTimePicker>());
        RegisterAppearance(FluentField.OutlineFor<AuDateTimePicker>());
        RegisterAppearance(FluentSpinner.Default);
        RegisterAppearance(new ClassAppearance<AuGroupBox>("au-group-box", "fl-group-box"));
    }

    public override IIconPainter IconPainter { get; } = new FluentIconPainter();
    public override FieldHeaderStyle HeaderStyle => FieldHeaderStyle.Static;
}
