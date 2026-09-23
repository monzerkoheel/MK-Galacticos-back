using MK.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.DTOs.Products
{
    public class ActiveProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public ProductAvailability AvailableFor { get; set; }
        public List<ActiveProductImageDto> MaleImages { get; set; } = new List<ActiveProductImageDto>();
        public List<ActiveProductImageDto> FemaleImages { get; set; } = new List<ActiveProductImageDto>();
        public decimal Price { get; set; }
        public decimal? SalePrice { get; set; } = null;
        public bool IsCustomPrintAvailable { get; set; }
        public List<ProductPrintDto> AvailablePrints { get; set; } = new List<ProductPrintDto>();
        public bool IsOutOfStock { get; set; }
        public List<ProductStockDto> Stock { get; set; } = new List<ProductStockDto>();
        public decimal AverageRating { get; set; }
        public int RatingCount { get; set; }
    }
}
