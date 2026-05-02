namespace Aurila.Models;

public readonly struct CssLength
{
    public static CssLength Auto => "auto";
    public static CssLength FitContent => "fit-content";
    public static CssLength MaxContent => "max-content";
    public static CssLength MinContent => "min-content";


    public double? Value { get; }
    public CssUnit? Unit { get; }
    public string? Raw { get; }

    public CssLength(double value, CssUnit unit = CssUnit.Pixels)
    {
        Value = value;
        Unit = unit;
        Raw = null;
    }

    private CssLength(string raw)
    {
        Raw = raw;
        Value = null;
        Unit = null;
    }

    public override string ToString()
    {
        if (Raw != null)
            return Raw;

        return Value!.Value.ToCssValue(Unit!.Value);
    }

    public static implicit operator CssLength(double value)
    {
        return new CssLength(value);
    }

    public static implicit operator CssLength(string value)
    {
        return new CssLength(value);
    }
}
