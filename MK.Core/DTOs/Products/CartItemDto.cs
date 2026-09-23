using MK.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.DTOs.Products
{
    public class CartItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string ProductMainImageUrl { get; set; } = null!;
        public decimal ProductPrice { get; set; }
        public decimal? ProductSalePrice { get; set; }
        public ProductGender Gender { get; set; }
        public ProductSize Size { get; set; }
        public ProductPrintType PrintType { get; set; }
        public string? CustomPrintName { get; set; }
        public int? CustomPrintNumber { get; set; }
        public int Quantity { get; set; }
    }
}
