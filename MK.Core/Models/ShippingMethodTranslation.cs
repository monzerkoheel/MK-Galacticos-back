using MK.Core.HelperModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.Models
{
    public class ShippingMethodTranslation : ModelBase
    {
        public int ShippingMethodId { get; set; }
        public Language Language { get; set; }
        public string Name { get; set; } = null!;
        public ShippingMethod ShippingMethod { get; set; } = null!;
    }
}
