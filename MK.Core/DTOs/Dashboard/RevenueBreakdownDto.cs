namespace MK.Core.DTOs.Dashboard
{
    public class RevenueBreakdownDto
    {
        /// <summary>Net revenue of the period. The items below add up to this number.</summary>
        public decimal TotalRevenue { get; set; }

        public List<RevenueBreakdownItemDto> Items { get; set; } = new();

        /// <summary>
        /// Discounts given in the period. Informational only: they are already deducted from the revenue,
        /// so they are not a slice of the donut.
        /// </summary>
        public decimal TotalDiscounts { get; set; }
    }

    public class RevenueBreakdownItemDto
    {
        public string Name { get; set; } = null!;
        public decimal Amount { get; set; }
        public decimal Percentage { get; set; }
    }
}
