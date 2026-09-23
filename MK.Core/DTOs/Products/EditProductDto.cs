using MK.Core.HelperModels;
using MK.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.DTOs.Products
{
    public class EditProductDto
    {
        public string? SKU { get; set; }
        public ProductType? ProductType { get; set; }
        public ProductAvailability? AvailableFor { get; set; }
        public Visibility? Visibility { get; set; }
        public decimal? Price { get; set; }
        public decimal? SalePrice { get; set; } = -1;
        public int? LowStockThreshold { get; set; }
        public bool? IsCustomPrintAvailable { get; set; }
    }
}
