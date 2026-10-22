using MK.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.DTOs.Orders
{
    public class OrderDetailsDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = null!;
        public string CustomerName { get; set; } = null!;
        public string CustomerEmail { get; set; } = null!;
        public OrderStatus Status { get; set; }
        public decimal Subtotal { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public bool HasCoupon { get; set; }
        public string? CouponCode { get; set; }
        public decimal? CouponDiscountAmount { get; set; }
        public bool HasLegendCard { get; set; }
        public decimal? LegendCardPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public OrderShippingDto Shipping { get; set; } = null!;
        public OrderPaymentDto Payment { get; set; } = null!;
        public ICollection<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
    }

    public class OrderShippingDto
    {
        public string? RecipientName { get; set; }
        public string Country { get; set; } = null!;
        public string Governorate { get; set; } = null!;
        public string Area { get; set; } = null!;
        public string DetailedAddress { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string? Notes { get; set; }
        public DateTime EstimatedDeliveryDate { get; set; }
        public DateTime? DeliveredAt { get; set; }
    }

    public class OrderPaymentDto
    {
        public PaymentStatus Status { get; set; }
        public decimal Amount { get; set; }
        public DateTime? PaidAt { get; set; }
    }

    public class OrderItemDto
    {
        public int? ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string ProductImageUrl { get; set; } = null!;
        public ProductGender Gender { get; set; }
        public ProductSize Size { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public bool IsCustomPrint { get; set; }
        public string? PrintName { get; set; }
        public int? PrintNumber { get; set; }
    }
}