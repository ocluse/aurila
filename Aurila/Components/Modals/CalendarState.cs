namespace Aurila.Components.Controls;

public record CalendarState
{
    public DateOnly? Value { get; init; }
    
    public DateOnly Today { get; init; }
    
    public DateOnly? Min { get; init; }
    
    public DateOnly? Max { get; init; }
    
    public DateTime? Date { get; init; }
    
    public DayOfWeek FirstDayOfWeek { get; init; }
    
    public int Month { get; set; }
    
    public int Year { get; set; }
}
