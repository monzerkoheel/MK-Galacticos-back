using MK.Core.HelperModels;
using MK.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.DTOs.Products
{
    public enum ProductPrintType
    {
        None,
        Custom,
        Players
    }
    public class ProductsDto
    {
        public int Id { get; set; } 
        public string Name { get; set; } = null!;
        public string SKU { get; set; } = null!;
        public string MainImageUrl { get; set; } = null!;
        public ICollection<string>? Collections { get; set; } = new List<string>();
        public ProductAvailability Gender { get; set; }
        public ProductPrintType PrintType { get; set; }
        public ICollection<string>? Prints { get; set; } = new List<string>();
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public Visibility Status { get; set; }
    }
}
