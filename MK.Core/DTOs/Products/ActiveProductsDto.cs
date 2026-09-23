using MK.Core.HelperModels;
using MK.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.DTOs.Products
{
    public class ActiveProductsDto
    {
        public int Id { get; set; }
        public string MainImageUrl { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string CollectionName { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string AvailableFor { get; set; } = null!;
        public decimal Price { get; set; }
        public decimal? SalePrice { get; set; }
        public bool IsNewArrival { get; set; }
        public bool IsOutOfStock { get; set; }
        public decimal AverageRating { get; set; }
        public int RatingCount { get; set; }
    }
}
