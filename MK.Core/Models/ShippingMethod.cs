using MK.Core.HelperModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.Models
{
    public class ShippingMethod : ModelBase
    {
        public decimal Price { get; set; }
        public int EstimatedDeliveryDays { get; set; }
        public Visibility Status { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }


        public ICollection<OrderShipping> OrderShippings { get; set; } = new List<OrderShipping>();
        public ICollection<ShippingMethodTranslation> Translations { get; set; } = new List<ShippingMethodTranslation>();
    }
}
