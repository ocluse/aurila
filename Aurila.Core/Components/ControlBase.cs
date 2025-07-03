using Aurila.Appearance;
using Aurila.Contracts.Components;

namespace Aurila.Components;

public class ControlBase<TControl> : ComponentBase, IControlComponent 
    where TControl : ControlBase<TControl>
{
    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public string? Style { get; set; }

    [Parameter]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    [Parameter]
    public string? Title { get; set; }

    [Parameter]
    public string? Id { get; set; }

    [Parameter]
    public IAppearance<TControl>? Appearance { get; set; }

    [CascadingParameter]
    public IAppearanceProvider? AppearanceProvider { get; set; }

    protected virtual void BuildStyle(StyleBuilder builder) { }

    protected virtual void BuildClass(ClassBuilder builder) { }

    protected virtual void BuildAttributes(IDictionary<string, object> attributes) { }

    protected string GetAppliedClass()
    {
        var classBuilder = new ClassBuilder();
        BuildClass(classBuilder);
        
        var effectiveAppearance = GetEffectiveAppearance();

        if (effectiveAppearance is IBuildingAppearance<TControl> buildingAppearance)
        {
            buildingAppearance.BuildClass((TControl)this, classBuilder);
        }
        else if (effectiveAppearance is IStaticAppearance<TControl> staticAppearance)
        {
            classBuilder.Add(staticAppearance.Class);
        }

        classBuilder.Add(Class);

        return classBuilder.Build();
    }

    protected string GetAppliedStyle()
    {
        var styleBuilder = new StyleBuilder();
        BuildStyle(styleBuilder);
        string builtStyle = styleBuilder.Build();

        var effectiveAppearance = GetEffectiveAppearance();

        if (effectiveAppearance is IBuildingAppearance<TControl> buildingAppearance)
        {
            buildingAppearance.BuildStyle((TControl)this, styleBuilder);
        }
        else if (effectiveAppearance is IStaticAppearance<TControl> staticAppearance)
        {
            if(!string.IsNullOrWhiteSpace(staticAppearance.Style))
            {
                if (builtStyle.Length > 0 && !builtStyle.EndsWith(';'))
                {
                    builtStyle += "; ";
                }

                builtStyle += staticAppearance.Style;
            }
        }

        if (!string.IsNullOrWhiteSpace(Style))
        {
            if (builtStyle.Length > 0 && !builtStyle.EndsWith(';'))
            {
                builtStyle += "; ";
            }
            builtStyle += Style;
        }

        return builtStyle.TrimEnd(';');
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
        if (!string.IsNullOrWhiteSpace(Title))
        {
            attributes["title"] = Title;
        }
        if (!string.IsNullOrWhiteSpace(Id))
        {
            attributes["id"] = Id;
        }
        BuildAttributes(attributes);
        if (AdditionalAttributes != null)
        {
            foreach (var kvp in AdditionalAttributes)
            {
                attributes[kvp.Key] = kvp.Value;
            }
        }
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
}
