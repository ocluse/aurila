using Aurila.Contracts.Navigation;
using Aurila.Enums.Input;
using Aurila.Models.Navigation;
using Aurila.Services.Navigation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Aurila;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAurila(this IServiceCollection services)
    {
        services.TryAddScoped<AurilaJSInterop>();
        services.TryAddScoped<IImageLoader, DefaultImageLoader>();
        services.TryAddSingleton<IRouteRegistry, RouteRegistry>();
        return services;
    }

    public static IServiceCollection AddAurilaRouting(this IServiceCollection services, Action<AurilaRoutingOptions> configureOptions)
    {
        services.Configure(configureOptions);

        return services;
    }
}

public static class Extensions
{
    public static string? GetDisplayValue<T>(this T? value, Func<T?, string>? displayMemberFunc)
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

    public static string ToHtmlAttribute(this UpdateTrigger trigger)
    {
        return trigger switch
        {
            UpdateTrigger.OnChange => "onchange",
            UpdateTrigger.OnInput => "oninput",
            _ => throw new ArgumentOutOfRangeException(nameof(trigger), trigger, null)
        };
    }

    public static string ToCssValue(this double value, CssUnit unit)
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

    public static string ToCssValue(this TextTransform value)
    {
        return value switch
        {
            TextTransform.None => "none",
            TextTransform.Uppercase => "uppercase",
            TextTransform.Lowercase => "lowercase",
            TextTransform.Capitalize => "capitalize",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        };
    }

    public static string ToCssValue(this TextDecoration value)
    {
        return value switch
        {
            TextDecoration.None => "none",
            TextDecoration.Underline => "underline",
            TextDecoration.Overline => "overline",
            TextDecoration.LineThrough => "line-through",
            TextDecoration.Blink => "blink",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        };
    }

    public static string ToCssValue(this FontWeight value)
    {
        return ((int)value).ToString();
    }

    public static string ToCssValue(this TextAlign value)
    {
        return value switch
        {
            TextAlign.Start => "start",
            TextAlign.End => "end",
            TextAlign.Center => "center",
            TextAlign.Justify => "justify",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        };
    }

    public static string ToContentType(this ImageFormat format)
    {
        return format switch
        {
            ImageFormat.Png => "image/png",
            ImageFormat.Jpeg => "image/jpeg",
            ImageFormat.Webp => "image/webp",
            ImageFormat.Gif => "image/gif",
            ImageFormat.Svg => "image/svg+xml",
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
    }

}

public static class CssUnitExtensions
{
    public static CssLength Px(this double value)
    {
        return new CssLength(value, CssUnit.Pixels);
    }

    public static CssLength Pc(this double value)
    {
        return new CssLength(value, CssUnit.Percent);
    }

    public static CssLength Em(this double value)
    {
        return new CssLength(value, CssUnit.Element);
    }

    public static CssLength Rem(this double value)
    {
        return new CssLength(value, CssUnit.RootElement);
    }

    public static CssLength Vw(this double value)
    {
        return new CssLength(value, CssUnit.ViewWidth);
    }

    public static CssLength Vh(this double value)
    {
        return new CssLength(value, CssUnit.ViewHeight);
    }

    public static CssLength Vmin(this double value)
    {
        return new CssLength(value, CssUnit.ViewMin);
    }

    public static CssLength Vmax(this double value)
    {
        return new CssLength(value, CssUnit.ViewMax);
    }

    public static CssLength Fr(this double value)
    {
        return new CssLength(value, CssUnit.Fraction);
    }

    public static CssLength Dvh(this double value)
    {
        return new CssLength(value, CssUnit.DynamicViewHeight);
    }
    public static CssLength Dvw(this double value)
    {
        return new CssLength(value, CssUnit.DynamicViewWidth);
    }

    public static CssLength Px(this int value) => ((double)value).Px();
    public static CssLength Pc(this int value) => ((double)value).Pc();
    public static CssLength Em(this int value) => ((double)value).Em();
    public static CssLength Rem(this int value) => ((double)value).Rem();
    public static CssLength Vw(this int value) => ((double)value).Vw();
    public static CssLength Vh(this int value) => ((double)value).Vh();
    public static CssLength Vmin(this int value) => ((double)value).Vmin();
    public static CssLength Vmax(this int value) => ((double)value).Vmax();
    public static CssLength Fr(this int value) => ((double)value).Fr();
    public static CssLength Dvh(this int value) => ((double)value).Dvh();
    public static CssLength Dvw(this int value) => ((double)value).Dvw();
}
