using Aurila.Contracts.Components;
using Ocluse.LiquidSnow.Extensions;
using Ocluse.LiquidSnow.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurila.Components.Controls;

public class Dropdown<TValue> : FieldBase<Dropdown<TValue>, TValue>, ICollectionView<TValue>, IAuxiliaryContentFieldComponent
{
    private readonly string _anchorName = "--" + IdGenerator.GenerateId(IdKind.Standard, 6).ToLowerInvariant();

    private ElementReference _popoverElement;

    [Parameter]
    public IEnumerable<TValue>? Items { get; set; }

    [Parameter]
    public RenderFragment<TValue>? ItemTemplate { get; set; }

    [Parameter]
    public RenderFragment<(TValue Value, bool? Selected)>? AdvancedItemTemplate { get; set; }

    [Parameter]
    public bool? ClearOnSimilar { get; set; }

    [Parameter]
    public string? OpenClass { get; set; }

    [Parameter]
    public string? ClosedClass { get; set; }

    [Parameter]
    public string? ItemClass { get; set; }

    [Parameter]
    public string? PopoverClass { get; set; }

    [Parameter]
    public RenderFragment? PlaceholderContent { get; set; }

    [Parameter]
    public Func<TValue?, string>? ToStringFunc { get; set; }

    [Inject]
    private AurilaJSInterop JSInterop { get; set; } = default!;

    protected override void BuildClass(ClassBuilder classBuilder)
    {
        base.BuildClass(classBuilder);
        classBuilder.Add("au-dropdown");
    }

    protected override void BuildInput(RenderTreeBuilder builder)
    {
        builder.OpenElement(1, "div");
        {
            builder.AddAttribute(2, "class", "input");
            builder.AddAttribute(3, "onclick", EventCallback.Factory.Create(this, HandleDropdownClick));
            if (Value != null)
            {
                if (AdvancedItemTemplate != null)
                {
                    builder.AddContent(4, AdvancedItemTemplate((Value, null)));
                }
                else if (ItemTemplate != null)
                {
                    builder.AddContent(5, ItemTemplate(Value));
                }
                else
                {
                    builder.OpenElement(6, "span");
                    {
                        builder.AddContent(7, Value.GetDisplayValue(ToStringFunc));
                    }
                    builder.CloseElement();
                }
            }
            else
            {
                if (PlaceholderContent != null)
                {
                    builder.AddContent(8, PlaceholderContent);
                }
                else if (Placeholder.IsNotEmpty())
                {
                    builder.OpenElement(9, "span");
                    {
                        builder.AddAttribute(10, "class", "placeholder");
                        builder.AddContent(11, Placeholder);
                    }
                    builder.CloseElement();
                }
            }
        }
        builder.CloseElement();
    }

    public void BuildAuxiliaryContent(RenderTreeBuilder builder)
    {
        builder.OpenElement(1, "div");
        {
            builder.AddAttribute(2, "class", "au-dropdown__popup");
            builder.AddAttribute(3, "style", $"position-anchor: {_anchorName};");
            builder.AddAttribute(4, "popover");
            builder.AddElementReferenceCapture(5, (value) => _popoverElement = value);
            if (Items != null && Items.Any())
            {
                foreach (TValue item in Items)
                {
                    builder.OpenElement(6, "div");
                    {
                        builder.SetKey(item);

                        bool selected = EqualityComparer<TValue>.Default.Equals(item, Value);
                        string itemClass = new ClassBuilder()
                            .Add("au-dropdown__item")
                            .AddIf(selected, "selected")
                            .Build();

                        builder.AddAttribute(7, "class", itemClass);
                        builder.AddAttribute(8, "onclick", EventCallback.Factory.Create(this, async () => await HandleItemClickAsync(item)));

                        if (AdvancedItemTemplate != null)
                        {
                            builder.AddContent(9, AdvancedItemTemplate, (item, selected));
                        }
                        else if (ItemTemplate != null)
                        {
                            builder.AddContent(10, ItemTemplate, item);
                        }
                        else
                        {
                            builder.OpenElement(12, "span");
                            {
                                builder.AddContent(13, item.GetDisplayValue(ToStringFunc));
                            }
                            builder.CloseElement();
                        }
                    }
                    builder.CloseElement();
                }
            }
            builder.CloseElement();
        }
    }

    protected override void BuildAttributes(IDictionary<string, object> attributes)
    {
        base.BuildAttributes(attributes);
        attributes.Add("data-anchor-name", _anchorName);
    }

    protected override void BuildStyle(StyleBuilder styleBuilder)
    {
        base.BuildStyle(styleBuilder);
        styleBuilder.Add("anchor-name", _anchorName);
    }

    private async Task HandleDropdownClick()
    {
        await JSInterop.ShowPopoverAsync(_popoverElement);
    }

    private async Task HandleItemClickAsync(TValue item)
    {
        if (EqualityComparer<TValue>.Default.Equals(item, Value) && (ClearOnSimilar == true))
        {
            await NotifyValueChange(default);
        }
        else
        {
            await NotifyValueChange(item);
        }

        await JSInterop.HidePopoverAsync(_popoverElement);
    }
}
