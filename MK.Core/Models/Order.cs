using MK.Core.HelperModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.Models
{
    public enum OrderStatus
    {
        Pending,
        Processing,
        Shipped,
        Delivered,
        Cancelled,
        NotReceived,
        ReFunded
    }
    public class Order : ModelBase
    {
        public string OrderNumber { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public OrderStatus Status { get; set; }
        public decimal Subtotal { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }

        public bool HasCoupon { get; set; }
        public int? UserCouponId { get; set; }
        public string? CouponCode { get; set; }
        public decimal? CouponDiscountAmount { get; set; }

        public bool HasLegendCard { get; set; }
        public int? LegendCardId { get; set; }
        public decimal? LegendCardPrice { get; set; }


        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }



        public ApplicationUser User { get; set; } = null!;
        public OrderShipping Shipping { get; set; } = null!; 
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public Payment Payment { get; set; } = null!;
        public UserCoupon? UserCoupon { get; set; }
        public LegendCard? LegendCard { get; set; }
    }
}
