using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Aurila.Components.Controls;
public class NumberPicker<TValue> : TextBoxBase<NumberPicker<TValue>, TValue> where TValue : struct, INumber<TValue>
{
    [Parameter]
    public TValue? Min { get; set; }

    [Parameter]
    public TValue? Max { get; set; }

    protected override string GetInputType() => "number";

    protected override TValue GetValue(object? value)
    {
        string? s = value?.ToString();
        return string.IsNullOrWhiteSpace(s) ? Min ?? default : ParseValue(s) ?? default;
    }

    private TValue? ParseValue(string? value)
    {
        if (TValue.TryParse(value, CultureInfo.CurrentCulture, out var t))
        {
            if (Min != null && t.CompareTo(Min) < 0)
            {
                return Min;
            }
            else if (Max != null && t.CompareTo(Max) > 0)
            {
                return Max;
            }
            else
            {
                return t;
            }
        }
        else
        {
            return Value;
        }
    }
}

