using MK.Core.HelperModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.Models
{
    public class OrderItem : ModelBase
    {
        public int OrderId { get; set; }
        public int? ProductId { get; set; }
        public string ProductImageUrl { get; set; } = null!;
        public ProductGender Gender { get; set; }
        public ProductSize Size { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public bool IsCustomPrint { get; set; }
        public int? ProductPrintId { get; set; }
        public string? PrintName { get; set; }
        public int? PrintNumber { get; set; }


        public Order Order { get; set; } = null!;
        public Product Product { get; set; } = null!;
        public ProductPrint? ProductPrint { get; set; }
        public ICollection<OrderItemTranslation> Translations { get; set; } = new List<OrderItemTranslation>();
    }
}
