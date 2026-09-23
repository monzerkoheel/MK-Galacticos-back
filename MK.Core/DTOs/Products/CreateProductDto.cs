using MK.Core.HelperModels;
using MK.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.DTOs.Products
{
    public class CreateProductDto
    {
        [Required]
        public string Slug { get; set; } = null!;
        [Required]
        public string SKU { get; set; } = null!;
        public ProductType ProductType { get; set; } = ProductType.Jersey;
        [Required]
        public ProductAvailability AvailableFor { get; set; }
        [Required]
        public decimal Price { get; set; }
        public decimal? SalePrice { get; set; }
        public int LowStockThreshold { get; set; } = 10;
        public Visibility Visibility { get; set; } = Visibility.Inactive;

        public bool IsFeatured { get; set; } = false;
        public bool IsNewArrival { get; set; } = true;
        public bool IsOutOfStock { get; set; } = false;
        public bool IsCustomPrintAvailable { get; set; }

        public ICollection<AddProductCollectionDto> ProductCollections { get; set; } = new List<AddProductCollectionDto>();
        public ICollection<AddProductImageDto> Images { get; set; } = new List<AddProductImageDto>();
        public ICollection<AddProductStockDto> Stocks { get; set; } = new List<AddProductStockDto>();
        public ICollection<AddProductPrintDto> Prints { get; set; } = new List<AddProductPrintDto>();
        public ICollection<AddProductTranslationDto> Translations { get; set; } = new List<AddProductTranslationDto>();
    }
}
