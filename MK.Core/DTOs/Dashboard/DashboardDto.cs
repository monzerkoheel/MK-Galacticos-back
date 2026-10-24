namespace MK.Core.DTOs.Dashboard
{
    /// <summary>
    /// Everything the dashboard page needs except the sales chart (it has its own endpoint/filter).
    /// </summary>
    public class DashboardDto
    {
        public DashboardPeriodDto Period { get; set; } = null!;
        public DashboardKpisDto Kpis { get; set; } = null!;
        public RevenueBreakdownDto RevenueBreakdown { get; set; } = null!;
        public List<DashboardRecentOrderDto> RecentOrders { get; set; } = new();
        public List<DashboardFanRequestDto> LatestFanRequests { get; set; } = new();
        public List<DashboardLowStockDto> LowStockProducts { get; set; } = new();
    }

    /// <summary>
    /// All dates are inclusive calendar days (UTC).
    /// </summary>
    public class DashboardPeriodDto
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public DateTime PreviousFrom { get; set; }
        public DateTime PreviousTo { get; set; }
    }
}
