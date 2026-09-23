using MK.Core.HelperModels;
using System;

namespace MK.Core.DTOs.ShippingMethods
{
    public class ShippingMethodDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public int EstimatedDeliveryDays { get; set; }
        public Visibility Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
