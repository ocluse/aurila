using Microsoft.Extensions.Options;

namespace Aurila.Fluent.Services;

public sealed class FluentThemeService
{
    private readonly FluentThemeOptions _options;

    public FluentThemeService(IOptions<FluentThemeOptions> options)
    {
        _options = options.Value;
        Mode = _options.Mode;
        Theme = new FluentTheme(_options);
    }

    public event Action? Changed;
    public FluentTheme Theme { get; private set; }
    public FluentThemeMode Mode { get; private set; }

    public void SetMode(FluentThemeMode mode)
    {
        if (Mode == mode) return;
        Mode = mode;
        Changed?.Invoke();
    }

    public void SetSeed(string seed)
    {
        _options.Seed = seed;
        Theme = new FluentTheme(_options);
        Changed?.Invoke();
    }

    public void SetTheme(FluentTheme theme)
    {
        Theme = theme ?? throw new ArgumentNullException(nameof(theme));
        Changed?.Invoke();
    }
}
