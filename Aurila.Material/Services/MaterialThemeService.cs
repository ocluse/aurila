using Microsoft.Extensions.Options;

namespace Aurila.Material.Services;

/// <summary>Holds the active theme and raises <see cref="Changed"/> when it is swapped at runtime.</summary>
public sealed class MaterialThemeService
{
    private readonly MaterialThemeOptions _options;

    public MaterialThemeService(IOptions<MaterialThemeOptions> options)
    {
        _options = options.Value;
        Mode = _options.Mode;
        Theme = new MaterialTheme(_options);
    }

    public event Action? Changed;

    public MaterialTheme Theme { get; private set; }

    public ThemeMode Mode { get; private set; }

    public bool RippleEnabled => !_options.DisableRipple;

    public void SetMode(ThemeMode mode)
    {
        if (Mode == mode)
        {
            return;
        }

        Mode = mode;
        Changed?.Invoke();
    }

    /// <summary>Regenerates every role from a new brand colour.</summary>
    public void SetSeed(string seed, SchemeVariant? variant = null)
    {
        _options.Seed = seed;

        if (variant.HasValue)
        {
            _options.Variant = variant.Value;
        }

        Theme = new MaterialTheme(_options);
        Changed?.Invoke();
    }

    public void SetTheme(MaterialTheme theme)
    {
        Theme = theme ?? throw new ArgumentNullException(nameof(theme));
        Changed?.Invoke();
    }
}
