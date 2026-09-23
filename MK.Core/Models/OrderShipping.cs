using MK.Core.HelperModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.Models
{
    public class OrderShipping : ModelBase
    {
        public int OrderId { get; set; }
        public int ShippingMethodId { get; set; }
        public decimal ShippingCost { get; set; }
        public DateTime EstimatedDeliveryDate { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime CreatedAt { get; set; }


        public string? RecipientName { get; set; }
        public string Country { get; set; } = null!;
        public string Governorate { get; set; } = null!;
        public string Area { get; set; } = null!;
        public string DetailedAddress { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string? Notes { get; set; }


        public Order Order { get; set; } = null!;
        public ShippingMethod ShippingMethod { get; set; } = null!;
    }
}
