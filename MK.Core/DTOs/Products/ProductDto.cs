using MK.Core.HelperModels;
using MK.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.DTOs.Products
{
    public class ProductDto
    {
        public int Id { get; set; }
        public ICollection<ProductTranslationDto> Translations { get; set; } = new List<ProductTranslationDto>();
        public string Slug { get; set; } = null!;
        public string SKU { get; set; } = null!;
        public ProductType ProductType { get; set; } = ProductType.Jersey;
        public ProductAvailability AvailableFor { get; set; }
        public decimal Price { get; set; }
        public decimal? SalePrice { get; set; }
        public int LowStockThreshold { get; set; }
        public Visibility Visibility { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsNewArrival { get; set; }
        public bool IsOutOfStock { get; set; }
        public bool IsCustomPrintAvailable { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public decimal AverageRating { get; set; }
        public int RatingCount { get; set; }
        public ICollection<ProductCollectionDto> ProductCollections { get; set; } = new List<ProductCollectionDto>();
        public ICollection<ManagmentProductImageDto> MaleImages { get; set; } = new List<ManagmentProductImageDto>();
        public ICollection<ManagmentProductImageDto> FemaleImages { get; set; } = new List<ManagmentProductImageDto>();
        public ICollection<ManagmentProductStockDto> Stocks { get; set; } = new List<ManagmentProductStockDto>();
        public ICollection<ProductPrintDto> Prints { get; set; } = new List<ProductPrintDto>();
        public int WishlistItemsCount { get; set; }
        public int CartItemsCount { get; set; }
        public ICollection<ProductOrdersDto> Orders { get; set; } = new List<ProductOrdersDto>();
        public int TotalOrdersCount { get; set; }
        public ICollection<ManagmentProductRatingDto> Ratings { get; set; } = new List<ManagmentProductRatingDto>();
        public int WaitingReStockNotificationsCount { get; set; }
    }
}
