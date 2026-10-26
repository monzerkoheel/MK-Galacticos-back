using MK.Core.HelperModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.Models
{
    public enum ProductType
    {
        Jersey,
        KidsJersey,
        MascotFigure
    }
    public enum ProductAvailability
    {
        Men,
        Women,
        Unisex
    }
    public enum ProductGender
    {
        Men,
        Women
    }
    public class Product : ModelBase
    {
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

        // Navigation Properties
        public ICollection<ProductCollection> ProductCollections { get; set; } = new List<ProductCollection>();
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public ICollection<ProductStock> Stocks { get; set; } = new List<ProductStock>();
        public ICollection<ProductPrint> Prints { get; set; } = new List<ProductPrint>();
        public ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<ProductRating> Ratings { get; set; } = new List<ProductRating>();
        public ICollection<AvailabilityNotification> AvailabilityNotifications { get; set; } = new List<AvailabilityNotification>();
        public ICollection<ProductTranslation> Translations { get; set; } = new List<ProductTranslation>();
    }
}
