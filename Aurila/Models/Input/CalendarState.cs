using Aurila.Enums.Input;
using System.Globalization;

namespace Aurila.Models.Input;

public record CalendarState
{
    public DateOnly? Value { get; init; }
    
    public DateOnly Today { get; init; }
    
    public DateOnly? Min { get; init; }
    
    public DateOnly? Max { get; init; }
    
    public DayOfWeek FirstDayOfWeek { get; init; }
    
    public int Month { get; init; }
    
    public int Year { get; init; }
    
    public CalendarViewMode ViewMode { get; init; } = CalendarViewMode.Days;

    public static CalendarState Current => new()
    {
        Month = DateTime.Now.Month,
        Year = DateTime.Now.Year,
        Today = DateOnly.FromDateTime(DateTime.Now),
        FirstDayOfWeek = CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek
    };

    public CalendarState AddMonths(int months)
    {
        if (Month < 1 || Month > 12 || Year < 1 || Year > 9999)
            return this; // Fallback for uninitialized state

        var d = new DateOnly(Year, Month, 1).AddMonths(months);
        return this with { Month = d.Month, Year = d.Year };
    }

    public CalendarState AddYears(int years)
    {
        int newYear = Year + years;
        if (newYear < 1) newYear = 1;
        if (newYear > 9999) newYear = 9999;
        
        return this with { Year = newYear };
    }

    public CalendarState WithViewMode(CalendarViewMode viewMode)
    {
        return this with { ViewMode = viewMode };
    }
}
