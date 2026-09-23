using MK.Core.HelperModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.Models
{
    public class OrderItemTranslation : ModelBase
    {
        public int OrderItemId { get; set; }
        public Language Language { get; set; }
        public string ProductName { get; set; } = null!;
        public OrderItem OrderItem { get; set; } = null!;
    }
}
