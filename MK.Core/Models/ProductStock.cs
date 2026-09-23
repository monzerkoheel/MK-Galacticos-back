using MK.Core.HelperModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.Models
{
    public enum ProductSize
    {
        S,
        M,
        L,
        XL,
        XXL
    }
    public class ProductStock : ModelBase
    {
        public int ProductId { get; set; }
        public ProductSize Size { get; set; }
        public int StockQuantity { get; set; }
        public ProductGender StockType { get; set; }
        public Product Product { get; set; } = null!;
    }
}
