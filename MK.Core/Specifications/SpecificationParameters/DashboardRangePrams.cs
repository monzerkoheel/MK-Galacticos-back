namespace MK.Core.Specifications.SpecificationParameters
{
    public class DashboardRangePrams
    {
        /// <summary>First day of the period. Default: 6 days before To (7 days in total).</summary>
        public DateTime? From { get; set; }

        /// <summary>Last day of the period (inclusive). Default: today (UTC).</summary>
        public DateTime? To { get; set; }
    }
}
