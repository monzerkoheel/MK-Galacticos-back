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
    public enum ActiveProductsSort
    {
        Price,
        Id
    }
    public class ActiveProductsSpecPrams
    {
        public string? Search { get; set; } = null;
        public ICollection<int>? CollectionIds { get; set; } = new List<int>();
        public ProductAvailability? Gender { get; set; } = null;
        public ICollection<ProductSize>? ProductSizes { get; set; } = new List<ProductSize>();
        public decimal? MinPrice { get; set; } = null;
        public decimal? MaxPrice { get; set; } = null;
        public ActiveProductsSort? Sort { get; set; } = null;
        public ActiveProductsSort? DesSort { get; set; } = null;
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
