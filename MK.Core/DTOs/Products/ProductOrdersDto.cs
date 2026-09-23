using MK.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.DTOs.Products
{
    public class ProductOrdersDto
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public string UserFullName { get; set; } = null!;
        public string? UserImageUrl { get; set; }
        public OrderStatus Status { get; set; }
        public int ShippingMethodId { get; set; }
        public string ShippingMethod { get; set; } = null!;
        public decimal TotalAmount { get; set; }
        public DateTime LastUpdateDate { get; set; }
    }
}
