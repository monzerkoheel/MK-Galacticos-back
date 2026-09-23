using MK.Core.HelperModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.Models
{
    public class CartItem : ModelBase
    {
        public int CartId { get; set; }
        public int ProductId { get; set; }
        public ProductGender Gender { get; set; }
        public ProductSize Size { get; set; }
        public bool IsCustomPrint { get; set; }
        public int? ProductPrintId { get; set; }
        public string? CustomPrintName { get; set; }
        public int? CustomPrintNumber { get; set; }
        public int Quantity { get; set; }


        public Cart Cart { get; set; } = null!;
        public Product Product { get; set; } = null!;
        public ProductPrint? ProductPrint { get; set; }
    }
}
