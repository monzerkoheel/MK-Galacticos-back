namespace MK.Core.DTOs.Dashboard
{
    public class DashboardKpisDto
    {
        /// <summary>Net revenue in the selected period vs the previous period.</summary>
        public DashboardMetricDto TotalRevenue { get; set; } = null!;

        /// <summary>Orders placed in the selected period vs the previous period.</summary>
        public DashboardMetricDto TotalOrders { get; set; } = null!;

        /// <summary>Total customers up to the end of the period. Change = growth of the total during the period.</summary>
        public DashboardMetricDto TotalCustomers { get; set; } = null!;

        /// <summary>Total products up to the end of the period. Change = growth of the total during the period.</summary>
        public DashboardMetricDto TotalProducts { get; set; } = null!;

        /// <summary>Total fan requests (any status) up to the end of the period. Change = growth of the total.</summary>
        public DashboardMetricDto FanRequests { get; set; } = null!;

        /// <summary>Value = orders that are Pending right now. Change compares pending orders placed in this period vs the previous one.</summary>
        public DashboardMetricDto PendingOrders { get; set; } = null!;

        public DashboardActiveCouponsDto ActiveCoupons { get; set; } = null!;

        /// <summary>Products whose total stock is above 0 and at or below their LowStockThreshold.</summary>
        public int LowStockProducts { get; set; }
    }

    public class DashboardMetricDto
    {
        public decimal Value { get; set; }

        /// <summary>
        /// Percent change (1 decimal). Null when the previous value is 0 and the current value is not (no base to compare to).
        /// </summary>
        public decimal? ChangePercent { get; set; }
    }

    public class DashboardActiveCouponsDto
    {
        public int Value { get; set; }

        /// <summary>Active coupons created inside the selected period ("2 new this week").</summary>
        public int NewInPeriod { get; set; }
    }
}
