using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.DTOs.Products
{
    public class RestockProductStockDto
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public int StockQuantity { get; set; }
    }
}
