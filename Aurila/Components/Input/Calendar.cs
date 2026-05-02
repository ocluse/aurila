using Aurila.Design;
using Aurila.Enums.Input;
using Aurila.Models.Input;

namespace Aurila.Components.Input;

public class Calendar : InputBase<Calendar, DateOnly?>
{
    [Parameter]
    public DateOnly? Min { get; set; }

    [Parameter]
    public DateOnly? Max { get; set; }
    
    [Parameter]
    public DateOnly? Today { get; set; }
    
    [Parameter]
    public DayOfWeek? FirstDayOfWeek { get; set; }
    
    [Parameter]
    public int Month { get; set; } = DateTime.Now.Month;
    
    [Parameter]
    public int Year { get; set; } = DateTime.Now.Year;

    [Parameter]
    public RenderFragment<CalendarState>? HeaderContent { get; set; }
    
    [Parameter]
    public RenderFragment<CalendarState>? FooterContent { get; set; }
    
    [Parameter]
    public Action<ClassBuilder, CalendarState, DateOnly>? BuildDayClassFunc { get; set; }

    [Parameter]
    public RenderFragment<DayOfWeek>? DayLabelContent { get; set; }

    [Parameter]
    public DayLabelFormat LabelFormat { get; set; } = DayLabelFormat.Abbreviated;

    [Parameter]
    public RenderFragment<DateOnly>? DayContent { get; set; }

    [Parameter]
    public string? DayDateFormat { get; set; }

    [Parameter]
    public SameDateClickBehavior SameDateClick { get; set; } = SameDateClickBehavior.Toggle;

    [Parameter] 
    public Func<DateOnly, bool>? IsDateDisabled { get; set; }

    private ElementReference _calendarElement;

    protected override ElementReference? FocusElement => _calendarElement;

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var now = DateTime.Now;
        var today = Today ?? DateOnly.FromDateTime(now);
        var firstDayOfWeek = FirstDayOfWeek ?? System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
        var dtf = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat;
        var calendarViews = GetCalendarWeeksView(Month, Year, firstDayOfWeek);

        var calendarState = new CalendarState
        {
            Value = Value,
            Today = today,
            Min = Min,
            Max = Max,
            Date = now,
            FirstDayOfWeek = firstDayOfWeek,
            Month = Month,
            Year = Year,
        };

        builder.OpenElement(1, "div");
        builder.AddMultipleAttributes(2, GetAppliedAttributes());
        builder.AddAttribute(3, "role", "application");
        builder.AddElementReferenceCapture(40, reference => _calendarElement = reference);
        {
            builder.OpenElement(4, "div");
            builder.AddAttribute(5, "class", "au-calendar__view");
            {
                // Header
                builder.OpenElement(6, "div");
                builder.AddAttribute(7, "class", "au-calendar__header");
                {
                    if (HeaderContent != null)
                    {
                        builder.AddContent(8, HeaderContent(calendarState));
                    }
                }
                builder.CloseElement(); // div (header)

                // Body
                builder.OpenElement(9, "div");
                builder.AddAttribute(10, "class", "au-calendar__body");
                builder.AddAttribute(11, "role", "grid");
                {
                    // Day Labels
                    builder.OpenElement(12, "div");
                    builder.AddAttribute(13, "class", "au-calendar__day-labels");
                    builder.AddAttribute(14, "role", "row");
                    {
                        for (int i = 0; i < 7; i++)
                        {
                            DayOfWeek day = (DayOfWeek)(((int)firstDayOfWeek + i) % 7);
                            string dayName = day.ToString()[..3].ToLower();

                            builder.OpenElement(15, "div");
                            builder.AddAttribute(16, "class", $"au-calendar__day-label au--calendar__day-label-{dayName}");
                            builder.AddAttribute(17, "role", "columnheader");
                            builder.AddAttribute(18, "aria-label", dtf.GetDayName(day));
                            {
                                if (DayLabelContent != null)
                                {
                                    builder.AddContent(19, DayLabelContent(day));
                                }
                                else
                                {
                                    string label = LabelFormat switch
                                    {
                                        DayLabelFormat.Single => dtf.GetShortestDayName(day)[..1],
                                        DayLabelFormat.Shortest => dtf.GetShortestDayName(day),
                                        DayLabelFormat.Full => dtf.GetDayName(day),
                                        _ => dtf.GetAbbreviatedDayName(day),
                                    };
                                    builder.AddContent(19, label);
                                }
                            }
                            builder.CloseElement(); // div (day label)
                        }
                    }
                    builder.CloseElement(); // div (day labels)

                    // Days
                    builder.OpenElement(20, "div");
                    builder.AddAttribute(21, "class", "au-calendar__days");
                    {
                        DateOnly current = calendarViews.Start;

                        while (current <= calendarViews.End)
                        {
                            builder.OpenElement(22, "div");
                            builder.AddAttribute(23, "class", "au-calendar__week");
                            builder.AddAttribute(24, "role", "row");
                            {
                                for (int d = 0; d < 7; d++)
                                {
                                    string dayName = current.DayOfWeek.ToString()[..3].ToLower();
                                    bool isCurrentMonth = current.Month == Month && current.Year == Year;
                                    bool isToday = current == today;
                                    bool isSelected = current == Value;
                                    bool isDisabled = (Min.HasValue && current < Min.Value)
                                                   || (Max.HasValue && current > Max.Value)
                                                   || (IsDateDisabled?.Invoke(current) ?? false);

                                    var dayClassBuilder = new ClassBuilder();
                                    dayClassBuilder.Add("au-calendar__day");
                                    dayClassBuilder.Add($"au-calendar__day-{dayName}");
                                    dayClassBuilder.Add($"au-calendar__day-{current.Day}");

                                    if (!isCurrentMonth) dayClassBuilder.Add("au-calendar__day--outside");
                                    if (isToday) dayClassBuilder.Add("au-calendar__day--today");
                                    if (isSelected) dayClassBuilder.Add("au-calendar__day--selected");
                                    if (isDisabled) dayClassBuilder.Add("au-calendar__day--disabled");

                                    BuildDayClassFunc?.Invoke(dayClassBuilder, calendarState, current);

                                    var capturedDate = current;

                                    builder.OpenElement(25, "button");
                                    builder.AddAttribute(26, "class", dayClassBuilder.ToString());
                                    builder.AddAttribute(27, "role", "gridcell");
                                    builder.AddAttribute(28, "aria-label", current.ToString("dddd, MMMM d, yyyy"));
                                    builder.AddAttribute(29, "aria-selected", isSelected ? "true" : "false");
                                    builder.AddAttribute(30, "aria-current", isToday ? "date" : null);
                                    {
                                        if (isDisabled)
                                        {
                                            builder.AddAttribute(31, "disabled", true);
                                        }
                                        else
                                        {
                                            builder.AddAttribute(31, "onclick", EventCallback.Factory.Create(this, async () =>
                                            {
                                                if (isSelected)
                                                {
                                                    switch (SameDateClick)
                                                    {
                                                        case SameDateClickBehavior.Toggle: await NotifyValueChange(null); break;
                                                        case SameDateClickBehavior.Refire: await NotifyValueChange(capturedDate); break;
                                                        case SameDateClickBehavior.Ignore: break;
                                                    }
                                                }
                                                else
                                                {
                                                    await NotifyValueChange(capturedDate);
                                                }
                                            }));
                                        }

                                        if (DayContent != null)
                                        {
                                            builder.AddContent(32, DayContent(current));
                                        }
                                        else if (DayDateFormat != null)
                                        {
                                            builder.AddContent(32, current.ToString(DayDateFormat));
                                        }
                                        else
                                        {
                                            builder.AddContent(32, current.Day);
                                        }
                                    }
                                    builder.CloseElement(); // button (day)

                                    current = current.AddDays(1);
                                }
                            }
                            builder.CloseElement(); // div (week)
                        }
                    }
                    builder.CloseElement(); // div (days)
                }
                builder.CloseElement(); // div (body)

                // Footer
                builder.OpenElement(33, "div");
                builder.AddAttribute(34, "class", "au-calendar__footer");
                {
                    if (FooterContent != null)
                    {
                        builder.AddContent(35, FooterContent(calendarState));
                    }
                }
                builder.CloseElement(); // div (footer)
            }
            builder.CloseElement(); // div (view)
        }
        builder.CloseElement(); // div (root)
    }

    protected override void BuildClass(ClassBuilder builder)
    {
        base.BuildClass(builder);
        builder.Add("au-calendar");
    }

    public static CalendarWeeksView GetCalendarWeeksView(int month, int year, DayOfWeek firstDayOfWeek)
    {
        DateOnly monthStart = new(year, month, 1);
        DateOnly monthEnd = new(year, month, DateTime.DaysInMonth(year, month));

        int startDiff = ((int)monthStart.DayOfWeek - (int)firstDayOfWeek + 7) % 7;

        // Prevent underflow if the date is January, Year 1
        DateOnly calStart = (year == 1 && month == 1)
            ? DateOnly.MinValue
            : monthStart.AddDays(-startDiff);

        int lastDayOfWeek = ((int)firstDayOfWeek + 6) % 7;
        int endDiff = (lastDayOfWeek - (int)monthEnd.DayOfWeek + 7) % 7;

        // Prevent overflow if the date is December, Year 9999
        DateOnly calEnd = (year == 9999 && month == 12)
            ? DateOnly.MaxValue
            : monthEnd.AddDays(endDiff);

        int totalWeeks = (calStart.DayNumber - calEnd.DayNumber) / -7 + 1;

        return new CalendarWeeksView(calStart, calEnd, totalWeeks);
    }

    public record CalendarWeeksView(DateOnly Start, DateOnly End, int TotalWeeks);
}