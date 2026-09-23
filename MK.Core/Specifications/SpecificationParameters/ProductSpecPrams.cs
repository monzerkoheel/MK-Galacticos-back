using MK.Core.DTOs.Products;
using MK.Core.HelperModels;
using MK.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.Specifications.SpecificationParameters
{
    public enum StockFilter
    {
        InStock,
        LowStock,
        OutOfStock
    }
    public enum ProductsSort
    {
        Price,
        Stock
    }
    public class ProductSpecPrams
    {
        public string? Search { get; set; } = null;
        public int? CollectionId { get; set; } = null;
        public ProductAvailability? Gender { get; set; } = null;
        public Visibility? Status { get; set; } = null;
        public ProductPrintType? PrintType { get; set; } = null;
        public StockFilter? StockFilter { get; set; } = null;
        public ProductsSort? Sort { get; set; } = null;
        public ProductsSort? DesSort { get; set; } = null;
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 8;
    }
}
