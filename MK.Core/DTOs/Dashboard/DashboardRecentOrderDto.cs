using MK.Core.Models;

namespace MK.Core.DTOs.Dashboard
{
    public class DashboardRecentOrderDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = null!;
        public string CustomerName { get; set; } = null!;
        public string? CustomerImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public OrderStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
