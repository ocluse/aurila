namespace Aurila.Fluent.Colors;

/// <summary>
/// Fluent 2 semantic colour aliases. The roles intentionally follow Fluent UI's
/// web token names so component states can be expressed without raw colours.
/// </summary>
public sealed record FluentColorScheme
{
    public required string NeutralForeground1 { get; init; }
    public required string NeutralForeground1Hover { get; init; }
    public required string NeutralForeground1Pressed { get; init; }
    public required string NeutralForeground2 { get; init; }
    public required string NeutralForeground2Hover { get; init; }
    public required string NeutralForeground2Pressed { get; init; }
    public required string NeutralForeground2BrandHover { get; init; }
    public required string NeutralForeground2BrandPressed { get; init; }
    public required string NeutralForeground3 { get; init; }
    public required string NeutralForeground4 { get; init; }
    public required string NeutralForegroundDisabled { get; init; }
    public required string NeutralForegroundInverted { get; init; }
    public required string NeutralForegroundInverted2 { get; init; }
    public required string NeutralForegroundOnBrand { get; init; }

    public required string NeutralBackground1 { get; init; }
    public required string NeutralBackground1Hover { get; init; }
    public required string NeutralBackground1Pressed { get; init; }
    public required string NeutralBackground1Selected { get; init; }
    public required string NeutralBackground2 { get; init; }
    public required string NeutralBackground2Hover { get; init; }
    public required string NeutralBackground2Pressed { get; init; }
    public required string NeutralBackground2Selected { get; init; }
    public required string NeutralBackground3 { get; init; }
    public required string NeutralBackground3Hover { get; init; }
    public required string NeutralBackground3Pressed { get; init; }
    public required string NeutralBackground3Selected { get; init; }
    public required string NeutralBackground4 { get; init; }
    public required string NeutralBackground5 { get; init; }
    public required string NeutralBackground6 { get; init; }
    public required string NeutralBackgroundDisabled { get; init; }
    public required string NeutralBackgroundInverted { get; init; }
    public required string NeutralCardBackground { get; init; }
    public required string SubtleBackground { get; init; }
    public required string SubtleBackgroundHover { get; init; }
    public required string SubtleBackgroundPressed { get; init; }
    public required string SubtleBackgroundSelected { get; init; }
    public required string TransparentBackground { get; init; }
    public required string TransparentBackgroundHover { get; init; }
    public required string TransparentBackgroundPressed { get; init; }

    public required string NeutralStroke1 { get; init; }
    public required string NeutralStroke1Hover { get; init; }
    public required string NeutralStroke1Pressed { get; init; }
    public required string NeutralStroke2 { get; init; }
    public required string NeutralStroke3 { get; init; }
    public required string NeutralStrokeAccessible { get; init; }
    public required string NeutralStrokeAccessibleHover { get; init; }
    public required string NeutralStrokeAccessiblePressed { get; init; }
    public required string NeutralStrokeDisabled { get; init; }
    public required string NeutralStrokeFocus1 { get; init; }
    public required string NeutralStrokeFocus2 { get; init; }

    public required string BrandBackground { get; init; }
    public required string BrandBackgroundHover { get; init; }
    public required string BrandBackgroundPressed { get; init; }
    public required string BrandBackgroundSelected { get; init; }
    public required string BrandBackground2 { get; init; }
    public required string BrandBackground2Hover { get; init; }
    public required string BrandBackground2Pressed { get; init; }
    public required string BrandForeground1 { get; init; }
    public required string BrandForeground1Hover { get; init; }
    public required string BrandForeground1Pressed { get; init; }
    public required string BrandForeground2 { get; init; }
    public required string BrandForegroundLink { get; init; }
    public required string BrandStroke1 { get; init; }
    public required string BrandStroke2 { get; init; }
    public required string BrandStroke2Contrast { get; init; }

    public required string StatusDangerForeground1 { get; init; }
    public required string StatusDangerBackground1 { get; init; }
    public required string StatusDangerBackground2 { get; init; }
    public required string StatusSuccessForeground1 { get; init; }
    public required string StatusSuccessBackground1 { get; init; }
    public required string StatusWarningForeground1 { get; init; }
    public required string StatusWarningBackground1 { get; init; }

    public required string ShadowAmbient { get; init; }
    public required string ShadowKey { get; init; }
    public required string ShadowAmbientLighter { get; init; }
    public required string ShadowKeyLighter { get; init; }
    public required string Scrim { get; init; }

    public static FluentColorScheme Light(FluentBrandRamp brand) => new()
    {
        NeutralForeground1 = "#242424", NeutralForeground1Hover = "#242424", NeutralForeground1Pressed = "#242424",
        NeutralForeground2 = "#424242", NeutralForeground2Hover = "#242424", NeutralForeground2Pressed = "#242424",
        NeutralForeground2BrandHover = brand[80], NeutralForeground2BrandPressed = brand[70],
        NeutralForeground3 = "#616161", NeutralForeground4 = "#707070", NeutralForegroundDisabled = "#BDBDBD",
        NeutralForegroundInverted = "#FFFFFF", NeutralForegroundInverted2 = "#FFFFFF", NeutralForegroundOnBrand = "#FFFFFF",

        NeutralBackground1 = "#FFFFFF", NeutralBackground1Hover = "#F5F5F5", NeutralBackground1Pressed = "#E0E0E0", NeutralBackground1Selected = "#EBEBEB",
        NeutralBackground2 = "#FAFAFA", NeutralBackground2Hover = "#F0F0F0", NeutralBackground2Pressed = "#DBDBDB", NeutralBackground2Selected = "#E6E6E6",
        NeutralBackground3 = "#F5F5F5", NeutralBackground3Hover = "#EBEBEB", NeutralBackground3Pressed = "#D6D6D6", NeutralBackground3Selected = "#E0E0E0",
        NeutralBackground4 = "#F0F0F0", NeutralBackground5 = "#EBEBEB", NeutralBackground6 = "#E6E6E6",
        NeutralBackgroundDisabled = "#F0F0F0", NeutralBackgroundInverted = "#292929", NeutralCardBackground = "#FAFAFA",
        SubtleBackground = "transparent", SubtleBackgroundHover = "#F5F5F5", SubtleBackgroundPressed = "#E0E0E0", SubtleBackgroundSelected = "#EBEBEB",
        TransparentBackground = "transparent", TransparentBackgroundHover = "transparent", TransparentBackgroundPressed = "transparent",

        NeutralStroke1 = "#D1D1D1", NeutralStroke1Hover = "#C7C7C7", NeutralStroke1Pressed = "#B3B3B3",
        NeutralStroke2 = "#E0E0E0", NeutralStroke3 = "#F0F0F0",
        NeutralStrokeAccessible = "#616161", NeutralStrokeAccessibleHover = "#575757", NeutralStrokeAccessiblePressed = "#4D4D4D",
        NeutralStrokeDisabled = "#E0E0E0", NeutralStrokeFocus1 = "#FFFFFF", NeutralStrokeFocus2 = "#000000",

        BrandBackground = brand[80], BrandBackgroundHover = brand[70], BrandBackgroundPressed = brand[40], BrandBackgroundSelected = brand[60],
        BrandBackground2 = brand[160], BrandBackground2Hover = brand[150], BrandBackground2Pressed = brand[130],
        BrandForeground1 = brand[80], BrandForeground1Hover = brand[70], BrandForeground1Pressed = brand[60],
        BrandForeground2 = brand[70], BrandForegroundLink = brand[70], BrandStroke1 = brand[80], BrandStroke2 = brand[140], BrandStroke2Contrast = brand[140],

        StatusDangerForeground1 = "#B10E1C", StatusDangerBackground1 = "#FDF3F4", StatusDangerBackground2 = "#FDE7E9",
        StatusSuccessForeground1 = "#107C10", StatusSuccessBackground1 = "#F1FAF1",
        StatusWarningForeground1 = "#8A3707", StatusWarningBackground1 = "#FFF8F0",
        ShadowAmbient = "rgba(0,0,0,.12)", ShadowKey = "rgba(0,0,0,.14)",
        ShadowAmbientLighter = "rgba(0,0,0,.06)", ShadowKeyLighter = "rgba(0,0,0,.07)", Scrim = "rgba(0,0,0,.32)",
    };

    public static FluentColorScheme Dark(FluentBrandRamp brand) => new()
    {
        NeutralForeground1 = "#FFFFFF", NeutralForeground1Hover = "#FFFFFF", NeutralForeground1Pressed = "#FFFFFF",
        NeutralForeground2 = "#D6D6D6", NeutralForeground2Hover = "#FFFFFF", NeutralForeground2Pressed = "#FFFFFF",
        NeutralForeground2BrandHover = brand[100], NeutralForeground2BrandPressed = brand[90],
        NeutralForeground3 = "#ADADAD", NeutralForeground4 = "#999999", NeutralForegroundDisabled = "#5C5C5C",
        NeutralForegroundInverted = "#242424", NeutralForegroundInverted2 = "#242424", NeutralForegroundOnBrand = "#FFFFFF",

        NeutralBackground1 = "#292929", NeutralBackground1Hover = "#3D3D3D", NeutralBackground1Pressed = "#1F1F1F", NeutralBackground1Selected = "#383838",
        NeutralBackground2 = "#1F1F1F", NeutralBackground2Hover = "#333333", NeutralBackground2Pressed = "#141414", NeutralBackground2Selected = "#2E2E2E",
        NeutralBackground3 = "#141414", NeutralBackground3Hover = "#292929", NeutralBackground3Pressed = "#0A0A0A", NeutralBackground3Selected = "#242424",
        NeutralBackground4 = "#0A0A0A", NeutralBackground5 = "#000000", NeutralBackground6 = "#333333",
        NeutralBackgroundDisabled = "#141414", NeutralBackgroundInverted = "#FFFFFF", NeutralCardBackground = "#333333",
        SubtleBackground = "transparent", SubtleBackgroundHover = "#383838", SubtleBackgroundPressed = "#2E2E2E", SubtleBackgroundSelected = "#333333",
        TransparentBackground = "transparent", TransparentBackgroundHover = "transparent", TransparentBackgroundPressed = "transparent",

        NeutralStroke1 = "#666666", NeutralStroke1Hover = "#757575", NeutralStroke1Pressed = "#6B6B6B",
        NeutralStroke2 = "#525252", NeutralStroke3 = "#3D3D3D",
        NeutralStrokeAccessible = "#ADADAD", NeutralStrokeAccessibleHover = "#BDBDBD", NeutralStrokeAccessiblePressed = "#B3B3B3",
        NeutralStrokeDisabled = "#424242", NeutralStrokeFocus1 = "#000000", NeutralStrokeFocus2 = "#FFFFFF",

        BrandBackground = brand[70], BrandBackgroundHover = brand[80], BrandBackgroundPressed = brand[40], BrandBackgroundSelected = brand[60],
        BrandBackground2 = brand[20], BrandBackground2Hover = brand[40], BrandBackground2Pressed = brand[10],
        BrandForeground1 = brand[100], BrandForeground1Hover = brand[110], BrandForeground1Pressed = brand[90],
        BrandForeground2 = brand[110], BrandForegroundLink = brand[100], BrandStroke1 = brand[100], BrandStroke2 = brand[50], BrandStroke2Contrast = brand[50],

        StatusDangerForeground1 = "#FF99A4", StatusDangerBackground1 = "#3B0509", StatusDangerBackground2 = "#520810",
        StatusSuccessForeground1 = "#54B054", StatusSuccessBackground1 = "#052505",
        StatusWarningForeground1 = "#FCE100", StatusWarningBackground1 = "#4A3B00",
        ShadowAmbient = "rgba(0,0,0,.24)", ShadowKey = "rgba(0,0,0,.28)",
        ShadowAmbientLighter = "rgba(0,0,0,.12)", ShadowKeyLighter = "rgba(0,0,0,.14)", Scrim = "rgba(0,0,0,.56)",
    };

    public IEnumerable<KeyValuePair<string, string>> EnumerateRoles()
    {
        foreach (var property in typeof(FluentColorScheme).GetProperties())
        {
            if (property.PropertyType == typeof(string) && property.GetValue(this) is string value)
            {
                yield return new(property.Name, value);
            }
        }
    }
}
