using Aurila.Design;
using Aurila.Enums.Input;
using Aurila.Models.Input;

namespace Aurila.Components.Input;

public class AuCalendar : AuInputBase<AuCalendar, DateOnly?>, IHasMargin
{    
    [Parameter]
    public CalendarState State { get; set; } = CalendarState.Current;

    [Parameter]
    public EventCallback<CalendarState> StateChanged { get; set; }

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
    public Action<ClassBuilder, CalendarState, int, int>? BuildMonthClassFunc { get; set; }

    [Parameter]
    public RenderFragment<(int Month, int Year)>? MonthContent { get; set; }

    [Parameter]
    public Action<ClassBuilder, CalendarState, int>? BuildYearClassFunc { get; set; }

    [Parameter]
    public RenderFragment<int>? YearContent { get; set; }

    [Parameter]
    public SameDateClickBehavior SameDateClick { get; set; } = SameDateClickBehavior.Toggle;

    [Parameter] 
    public Func<DateOnly, bool>? IsDateDisabled { get; set; }

    [Parameter]
    public CssLength? Margin { get; set; }

    [Parameter]
    public CssLength? MarginHorizontal { get; set; }

    [Parameter]
    public CssLength? MarginVertical { get; set; }

    [Parameter]
    public CssLength? MarginRight { get; set; }

    [Parameter]
    public CssLength? MarginLeft { get; set; }

    [Parameter]
    public CssLength? MarginTop { get; set; }

    [Parameter]
    public CssLength? MarginBottom { get; set; }

    private ElementReference _calendarElement;

    protected override ElementReference? FocusElement => _calendarElement;

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var dtf = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat;
        var calendarViews = GetCalendarWeeksView(State.Month, State.Year, State.FirstDayOfWeek);

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
                        builder.AddContent(8, HeaderContent(State));
                    }
                }
                builder.CloseElement(); // div (header)

                // Body
                builder.OpenElement(9, "div");
                builder.AddAttribute(10, "class", "au-calendar__body");
                builder.AddAttribute(11, "role", "grid");
                {
                    if (State.ViewMode == CalendarViewMode.Days)
                    {
                        // Day Labels
                        builder.OpenElement(12, "div");
                        builder.AddAttribute(13, "class", "au-calendar__day-labels");
                        builder.AddAttribute(14, "role", "row");
                        {
                            for (int i = 0; i < 7; i++)
                            {
                                DayOfWeek day = (DayOfWeek)(((int)State.FirstDayOfWeek + i) % 7);
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
                        builder.AddAttribute(21, "class", "au-calendar__items au-calendar__days");
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
                                        bool isOutside = current.Month != State.Month || current.Year != State.Year;
                                        bool isToday = current == State.Today;
                                        bool isSelected = current == Value;
                                        bool isDisabled = (State.Min.HasValue && current < State.Min.Value)
                                                       || (State.Max.HasValue && current > State.Max.Value)
                                                       || (IsDateDisabled?.Invoke(current) ?? false);

                                        ClassBuilder classBuilder = new();
                                        
                                        //item
                                        classBuilder.Add("au-calendar__item");
                                        classBuilder.AddIf(isOutside, "au-calendar__item--outside");
                                        classBuilder.AddIf(isToday, "au-calendar__item--today");
                                        classBuilder.AddIf(isSelected, "au-calendar__item--selected");
                                        classBuilder.AddIf(isDisabled, "au-calendar__item--disabled");

                                        //day
                                        classBuilder.Add("au-calendar__day");
                                        classBuilder.Add($"au-calendar__day-{dayName}");
                                        classBuilder.Add($"au-calendar__day-{current.Day}");

                                        classBuilder.AddIf(isOutside, "au-calendar__day--outside");
                                        classBuilder.AddIf(isToday, "au-calendar__day--today");
                                        classBuilder.AddIf(isSelected, "au-calendar__day--selected");
                                        classBuilder.AddIf(isDisabled, "au-calendar__day--disabled");

                                        BuildDayClassFunc?.Invoke(classBuilder, State, current);

                                        var capturedDate = current;

                                        builder.OpenElement(25, "button");
                                        builder.AddAttribute(26, "class", classBuilder.ToString());
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
                    else if (State.ViewMode == CalendarViewMode.Months)
                    {
                        builder.OpenElement(50, "div");
                        builder.AddAttribute(51, "class", "au-calendar__items au-calendar__months");
                        {
                            var startDate = new DateOnly(State.Year, 1, 1).AddMonths(-2);

                            for (int i = 0; i < 16; i++)
                            {
                                var currentMonthDate = startDate.AddMonths(i);
                                int m = currentMonthDate.Month;
                                int y = currentMonthDate.Year;

                                bool isOutside = y != State.Year;
                                bool isToday = m == State.Today.Month && y == State.Today.Year;
                                bool isSelected = m == Value?.Month && y == Value?.Year;
                                bool isDisabled = (State.Min.HasValue && currentMonthDate < new DateOnly(State.Min.Value.Year, State.Min.Value.Month, 1))
                                               || (State.Max.HasValue && currentMonthDate > new DateOnly(State.Max.Value.Year, State.Max.Value.Month, 1));

                                ClassBuilder classBuilder = new();

                                //item
                                classBuilder.Add("au-calendar__item");
                                classBuilder.AddIf(isOutside, "au-calendar__item--outside");
                                classBuilder.AddIf(isToday, "au-calendar__item--today");
                                classBuilder.AddIf(isSelected, "au-calendar__item--selected");
                                classBuilder.AddIf(isDisabled, "au-calendar__item--disabled");

                                classBuilder.Add("au-calendar__month");
                                classBuilder.AddIf(isOutside, "au-calendar__month--outside");
                                classBuilder.AddIf(isToday, "au-calendar__month--today");
                                classBuilder.AddIf(isSelected, "au-calendar__month--selected");
                                classBuilder.AddIf(isDisabled, "au-calendar__month--disabled");

                                BuildMonthClassFunc?.Invoke(classBuilder, State, m, y);

                                builder.OpenElement(52, "button");
                                builder.AddAttribute(53, "class", classBuilder.ToString());
                                builder.AddAttribute(54, "role", "gridcell");
                                builder.AddAttribute(55, "aria-label", currentMonthDate.ToString("MMMM yyyy"));
                                builder.AddAttribute(56, "aria-selected", isSelected ? "true" : "false");
                                builder.AddAttribute(57, "onclick", EventCallback.Factory.Create(this, async () =>
                                {
                                    var newState = State with { Month = m, Year = y, ViewMode = CalendarViewMode.Days };
                                    await StateChanged.InvokeAsync(newState);
                                }));
                                {
                                    if (MonthContent != null)
                                    {
                                        builder.AddContent(58, MonthContent((m, y)));
                                    }
                                    else
                                    {
                                        builder.AddContent(58, dtf.GetAbbreviatedMonthName(m));
                                    }
                                }
                                builder.CloseElement(); // button
                            }
                        }
                        builder.CloseElement(); // div (months)
                    }
                    else if (State.ViewMode == CalendarViewMode.Years)
                    {
                        builder.OpenElement(60, "div");
                        builder.AddAttribute(61, "class", "au-calendar__items au-calendar__years");
                        {
                            int decadeStart = (State.Year / 10) * 10;
                            int startYear = decadeStart - 2;

                            for (int i = 0; i < 16; i++)
                            {
                                int y = startYear + i;

                                bool isOutside = y < decadeStart || y > decadeStart + 9;
                                bool isToday = y == DateTime.Now.Year;
                                bool isSelected = y == Value?.Year;
                                bool isDisabled = (State.Min.HasValue && y < State.Min.Value.Year)
                                               || (State.Max.HasValue && y > State.Max.Value.Year);

                                ClassBuilder classBuilder = new();

                                classBuilder.Add("au-calendar__item");
                                classBuilder.AddIf(isOutside, "au-calendar__item--outside");
                                classBuilder.AddIf(isToday, "au-calendar__item--today");
                                classBuilder.AddIf(isSelected, "au-calendar__item--selected");
                                classBuilder.AddIf(isDisabled, "au-calendar__item--disabled");

                                classBuilder.Add("au-calendar__year");
                                classBuilder.AddIf(isOutside, "au-calendar__year--outside");
                                classBuilder.AddIf(isToday, "au-calendar__year--today");
                                classBuilder.AddIf(isSelected, "au-calendar__year--selected");
                                classBuilder.AddIf(isDisabled, "au-calendar__year--disabled");

                                BuildYearClassFunc?.Invoke(classBuilder, State, y);

                                builder.OpenElement(62, "button");
                                builder.AddAttribute(63, "class", classBuilder.ToString());
                                builder.AddAttribute(64, "role", "gridcell");
                                builder.AddAttribute(65, "aria-label", y.ToString());
                                builder.AddAttribute(66, "aria-selected", isSelected ? "true" : "false");
                                builder.AddAttribute(67, "onclick", EventCallback.Factory.Create(this, async () =>
                                {
                                    var newState = State with { Year = y, ViewMode = CalendarViewMode.Months };
                                    await StateChanged.InvokeAsync(newState);
                                }));
                                {
                                    if (YearContent != null)
                                    {
                                        builder.AddContent(68, YearContent(y));
                                    }
                                    else
                                    {
                                        builder.AddContent(68, y.ToString());
                                    }
                                }
                                builder.CloseElement(); // button
                            }
                        }
                        builder.CloseElement(); // div (years)
                    }
                }
                builder.CloseElement(); // div (body)

                // Footer
                builder.OpenElement(70, "div");
                builder.AddAttribute(71, "class", "au-calendar__footer");
                {
                    if (FooterContent != null)
                    {
                        builder.AddContent(72, FooterContent(State));
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
