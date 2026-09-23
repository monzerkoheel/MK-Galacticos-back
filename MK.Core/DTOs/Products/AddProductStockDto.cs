using MK.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.DTOs.Products
{
    public class AddProductStockDto
    {
        [Required]
        public ProductSize Size { get; set; }
        [Required]
        public int StockQuantity { get; set; }
        [Required]
        public ProductGender StockType { get; set; }
    }
}
