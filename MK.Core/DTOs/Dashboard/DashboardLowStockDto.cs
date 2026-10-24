using MK.Core.Models;

namespace MK.Core.DTOs.Dashboard
{
    /// <summary>
    /// One row = one size of one product ("Real Madrid 1998 Home - Size M - 5 left").
    /// </summary>
    public class DashboardLowStockDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public ProductSize Size { get; set; }
        public ProductGender Gender { get; set; }
        public int Quantity { get; set; }
    }
}
