using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurila;
internal static class Extensions
{
    internal static string? GetDisplayValue<T>(this T? value, Func<T?, string>? displayMemberFunc)
    {
        if (displayMemberFunc != null)
        {
            return displayMemberFunc(value);
        }
        if (value == null)
        {
            return null;
        }
        return value.ToString();
    }

    internal static string ToHtmlAttribute(this UpdateTrigger trigger)
    {
        return trigger switch
        {
            UpdateTrigger.OnChange => "onchange",
            UpdateTrigger.OnInput => "oninput",
            _ => throw new ArgumentOutOfRangeException(nameof(trigger), trigger, null)
        };
    }

    internal static string ToCssValue(this double value, CssUnit unit)
    {
        return unit switch
        {
            CssUnit.Pixels => $"{value}px",
            CssUnit.Percent => $"{value}%",
            CssUnit.Element => $"{value}em",
            CssUnit.RootElement => $"{value}rem",
            CssUnit.ViewWidth => $"{value}vw",
            CssUnit.ViewHeight => $"{value}vh",
            CssUnit.ViewMin => $"{value}vmin",
            CssUnit.ViewMax => $"{value}vmax",
            CssUnit.Fraction => $"{value}fr",
            CssUnit.DynamicViewHeight => $"{value}dvh",
            CssUnit.DynamicViewWidth => $"{value}dvw",
            _ => throw new ArgumentOutOfRangeException(nameof(unit), unit, null)
        };
    }
}
