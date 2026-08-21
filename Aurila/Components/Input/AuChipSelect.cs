using Aurila.Contracts.Layout;
using Aurila.Design;
using Aurila.Components.Controls;
using Aurila.Enums.Input;
using Aurila.Models.Navigation;
using Microsoft.AspNetCore.Components.Web;

namespace Aurila.Components.Input;

public class AuChipSelect<TValue> : AuInputBase<AuChipSelect<TValue>, TValue>, ICollectionView<TValue>, IHasMargin
{
    [Parameter]
    public IEnumerable<TValue>? Items { get; set; }

    [Parameter]
    public RenderFragment<(TValue Item, bool Selected)>? ItemTemplateWithSelected { get; set; }

    [Parameter]
    public RenderFragment<TValue>? ItemTemplate { get; set; }

    /// <summary>
    /// Produces an optional navigation destination for each chip. An empty target leaves the item as
    /// a native action button.
    /// </summary>
    [Parameter]
    public Func<TValue, NavTarget>? GetItemTarget { get; set; }

    [Parameter]
    public RenderFragment? EmptyTemplate { get; set; }

    [Parameter]
    public Func<TValue?, string>? ToStringFunc { get; set; }

    [Parameter]
    public IReadOnlyCollection<TValue> SelectedItems { get; set; } = [];

    [Parameter]
    public EventCallback<IReadOnlyCollection<TValue>> SelectedItemsChanged { get; set; }

    [Parameter]
    public SelectionMode SelectionMode { get; set; }
    
    [Parameter]
    public CssLength? Margin { get; set; }

    [Parameter]
    public CssLength? MarginHorizontal { get; set; }

    [Parameter]
    public CssLength? MarginVertical { get; set; }

    [Parameter]
    public CssLength? MarginRight { get; set; }

    [Parameter]
    public CssLength? MarginLeft { get; set; }

    [Parameter]
    public CssLength? MarginTop { get; set; }

    [Parameter]
    public CssLength? MarginBottom { get; set; }

    private ElementReference _chipSelectElement;

    protected override ElementReference? FocusElement => _chipSelectElement;

    protected override void BuildClass(ClassBuilder builder)
    {
        builder.Add("au-chip-select")
            .AddIf(SelectionMode == SelectionMode.Single, "au-chip-select--single")
            .AddIf(SelectionMode == SelectionMode.SingleToggle, "au-chip-select--single-toggle")
            .AddIf(SelectionMode == SelectionMode.Multiple, "au-chip-select--multiple");
        base.BuildClass(builder);
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(1, "div");
        {
            builder.AddMultipleAttributes(2, GetAppliedAttributes());
            builder.AddElementReferenceCapture(11, reference => _chipSelectElement = reference);

            if (Items != null && Items.Any())
            {
                foreach (TValue item in Items)
                {
                    bool selected = SelectionMode == SelectionMode.Multiple
                        ? SelectedItems.Contains(item)
                        : EqualityComparer<TValue>.Default.Equals(item, Value);
                    NavTarget target = GetItemTarget?.Invoke(item) ?? default;

                    builder.OpenComponent<AuChip>(3);
                    builder.SetKey(item);
                    builder.AddAttribute(4, nameof(AuChip.Class), "au-chip-select__item");
                    builder.AddAttribute(5, nameof(AuChip.Selected), selected);
                    builder.AddAttribute(6, nameof(AuChip.Selectable), true);
                    builder.AddAttribute(7, nameof(AuChip.Disabled), Disabled);
                    builder.AddAttribute(8, nameof(AuChip.To), target);
                    builder.AddAttribute(
                        9,
                        nameof(AuChip.Clicked),
                        EventCallback.Factory.Create<MouseEventArgs>(
                            this,
                            e => HandleItemClickAsync(item, e, !target.IsEmpty)));
                    builder.AddAttribute(
                        10,
                        nameof(AuChip.ChildContent),
                        (RenderFragment)(chipBuilder => BuildItemContent(chipBuilder, item, selected)));
                    builder.CloseComponent();
                }
            }
            else if (EmptyTemplate != null)
            {
                builder.AddContent(10, EmptyTemplate);
            }
        }
        builder.CloseElement();
    }

    private void BuildItemContent(RenderTreeBuilder builder, TValue item, bool selected)
    {
        if (ItemTemplateWithSelected != null)
        {
            builder.AddContent(0, ItemTemplateWithSelected, (item, selected));
        }
        else if (ItemTemplate != null)
        {
            builder.AddContent(1, ItemTemplate, item);
        }
        else
        {
            builder.OpenElement(2, "span");
            builder.AddContent(3, item.GetDisplayValue(ToStringFunc));
            builder.CloseElement();
        }
    }

    private async Task HandleItemClickAsync(TValue value, MouseEventArgs e, bool isLink)
    {
        if (isLink
            && (e.Button != 0 || e.CtrlKey || e.MetaKey || e.ShiftKey || e.AltKey))
        {
            return;
        }

        await NotifyValueChange(value);

        if (SelectionMode == SelectionMode.Single)
        {
            await SelectedItemsChanged.InvokeAsync([value]);
        }
        else if (SelectionMode == SelectionMode.SingleToggle)
        {
            if (EqualityComparer<TValue>.Default.Equals(Value, value))
            {
                await SelectedItemsChanged.InvokeAsync([]);
            }
            else
            {
                await SelectedItemsChanged.InvokeAsync([value]);
            }
        }
        else
        {
            List<TValue> selectedItems = [.. SelectedItems];
            if (!selectedItems.Remove(value))
            {
                selectedItems.Add(value);
            }
            await SelectedItemsChanged.InvokeAsync(selectedItems);
        }
    }
}
