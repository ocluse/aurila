using Aurila.Contracts.Components;

namespace Aurila.Components.Controls;

public class ChipSelect<TValue> : InputBase<ChipSelect<TValue>, TValue>, ICollectionView<TValue>
{
    [Parameter]
    public IEnumerable<TValue>? Items { get; set; }

    [Parameter]
    public RenderFragment<(TValue Item, bool Selected)>? ItemTemplateWithSelected { get; set; }

    [Parameter]
    public RenderFragment<TValue>? ItemTemplate { get; set; }

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

            if (Items != null && Items.Any())
            {
                foreach (TValue item in Items)
                {
                    builder.OpenElement(3, "div");
                    {
                        builder.SetKey(item);

                        bool selected = SelectionMode == SelectionMode.Multiple ? SelectedItems.Contains(item) : EqualityComparer<TValue>.Default.Equals(item, Value);

                        string itemClass = new ClassBuilder()
                            .Add("au-chip au-chip-select__item")
                            .AddIf(selected, "au-chip--selected")
                            .Build();

                        builder.AddAttribute(4, "class", itemClass);
                        builder.AddAttribute(5, "onclick", EventCallback.Factory.Create(this, async () => await HandleItemClickAsync(item)));

                        if (ItemTemplateWithSelected != null)
                        {
                            builder.AddContent(6, ItemTemplateWithSelected, (item, selected));
                        }
                        else if (ItemTemplate != null)
                        {
                            builder.AddContent(7, ItemTemplate, item);
                        }
                        else
                        {
                            builder.OpenElement(8, "span");
                            {
                                builder.AddContent(9, item.GetDisplayValue(ToStringFunc));
                            }
                            builder.CloseElement();
                        }
                    }
                    builder.CloseElement();
                }
            }
            else if (EmptyTemplate != null)
            {
                builder.AddContent(10, EmptyTemplate);
            }
        }
        builder.CloseElement();
    }

    private async Task HandleItemClickAsync(TValue value)
    {
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