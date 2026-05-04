namespace Aurila.Models.Layout;

public record GridDefinition(string Columns, string Rows = "auto")
{
    public static GridDefinition Cols(params string[] columns)
        => new(string.Join(" ", columns));

    public static GridDefinition Grid(string columns, string rows)
        => new(columns, rows);
}