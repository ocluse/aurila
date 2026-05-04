using Aurila.Contracts.Design.Appearance;
using Aurila.Contracts.Layout;
using Aurila.Design;
using Aurila.Design.Modifiers;

namespace Aurila.Components;

public class AuControlBase<TControl> : ComponentBase, IControlComponent, ILayoutChild
    where TControl : AuControlBase<TControl>
{
    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public Action<ClassBuilder>? ClassBuilder { get; set; }

    [Parameter]
    public string? Style { get; set; }

    [Parameter]
    public Action<StyleBuilder>? StyleBuilder { get; set; }

    [Parameter]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    [Parameter]
    public string? HtmlTitleAttr { get; set; }

    [Parameter]
    public string? HtmlIdAttr { get; set; }

    [Parameter]
    public IAppearance<TControl>? Appearance { get; set; }

    [Parameter]
    public ModifiersBuilder? Modifier { get; set; }

    [CascadingParameter]
    public IAppearanceProvider? AppearanceProvider { get; set; }

    [CascadingParameter]
    public ILayoutParent? LayoutParent { get; set; }

    ILayoutParent? ILayoutChild.Parent => LayoutParent;

    protected virtual void BuildStyle(StyleBuilder builder) { }

    protected virtual void BuildClass(ClassBuilder builder) { }

    protected virtual void BuildAttributes(IDictionary<string, object> attributes) { }

    protected string GetAppliedClass()
    {
        var classBuilder = new ClassBuilder();
        
        //Applies classes from appearances:
        var effectiveAppearance = GetEffectiveAppearance();

        if (effectiveAppearance is IBuildingAppearance<TControl> buildingAppearance)
        {
            buildingAppearance.BuildClass((TControl)this, classBuilder);
        }
        else if (effectiveAppearance is IStaticAppearance<TControl> staticAppearance)
        {
            classBuilder.Add(staticAppearance.Class);
        }

        //Component:
        BuildClass(classBuilder);
        StylingUtility.BuildClass(this, classBuilder);

        // Apply modifiers
        Modifier?.BuildClass(this, classBuilder);

        // Customs:
        classBuilder.Add(Class);
        ClassBuilder?.Invoke(classBuilder);

        return classBuilder.ToString();
    }

    protected string GetAppliedStyle()
    {
        var styleBuilder = new StyleBuilder();

        //Apply styles from appearances:
        var effectiveAppearance = GetEffectiveAppearance();

        if (effectiveAppearance is IBuildingAppearance<TControl> buildingAppearance)
        {
            buildingAppearance.BuildStyle((TControl)this, styleBuilder);
        }

        if (effectiveAppearance is IStaticAppearance<TControl> staticAppearance)
        {
            var styles = StylingUtility.GetStyles(staticAppearance.Style);

            foreach (var style in styles)
            {
                styleBuilder.Add(style.Key, style.Value);
            }
        }

        // Apply styles from the component itself:
        BuildStyle(styleBuilder);
        StylingUtility.BuildStyle(this, styleBuilder);

        //Apply modifiers:
        Modifier?.BuildStyle(this, styleBuilder);

        //Apply custom styles:
        if (!string.IsNullOrWhiteSpace(Style))
        {
            var styles = StylingUtility.GetStyles(Style);
            foreach (var style in styles)
            {
                styleBuilder.Add(style.Key, style.Value);
            }
        }

        StyleBuilder?.Invoke(styleBuilder);

        //Build style:
        return styleBuilder.ToString();
    }

    IEnumerable<KeyValuePair<string, object>> IControlComponent.GetAppliedAttributes()
    {
        return GetAppliedAttributes();
    }

    protected Dictionary<string, object> GetAppliedAttributes()
    {
        var attributes = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        string appliedClass = GetAppliedClass();
        if (!string.IsNullOrWhiteSpace(appliedClass))
        {
            attributes["class"] = appliedClass;
        }
        string appliedStyle = GetAppliedStyle();
        if (!string.IsNullOrWhiteSpace(appliedStyle))
        {
            attributes["style"] = appliedStyle;
        }
        if (!string.IsNullOrWhiteSpace(HtmlTitleAttr))
        {
            attributes["title"] = HtmlTitleAttr;
        }
        if (!string.IsNullOrWhiteSpace(HtmlIdAttr))
        {
            attributes["id"] = HtmlIdAttr;
        }
        BuildAttributes(attributes);
        if (AdditionalAttributes != null)
        {
            foreach (var kvp in AdditionalAttributes)
            {
                attributes[kvp.Key] = kvp.Value;
            }
        }

        //modifier attributes:
        Modifier?.BuildAttributes(this, attributes);

        return attributes;
    }

    private IAppearance<TControl>? GetEffectiveAppearance()
    {
        if (Appearance != null)
        {
            return Appearance;
        }
        return AppearanceProvider?.GetAppearance<TControl>();
    }

    public async Task CallStateHasChangedOnContextAsync()
    {
        await InvokeAsync(StateHasChanged);
    }

    public void CallStateHasChanged()
    {
        StateHasChanged();
    }
}
