namespace MK.Core.DTOs.Dashboard
{
    public enum SalesOverviewRange
    {
        /// <summary>Last 7 days including today, one point per day.</summary>
        Last7Days = 0,

        /// <summary>Last 30 days including today, one point per day.</summary>
        Last30Days = 1,

        /// <summary>Last 12 months including the current one, one point per month.</summary>
        Last12Months = 2
    }

    public class SalesOverviewDto
    {
        public SalesOverviewRange Range { get; set; }
        public List<SalesOverviewPointDto> Points { get; set; } = new();
    }

    public class SalesOverviewPointDto
    {
        /// <summary>The day, or the first day of the month for Last12Months.</summary>
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int Orders { get; set; }
    }
}
