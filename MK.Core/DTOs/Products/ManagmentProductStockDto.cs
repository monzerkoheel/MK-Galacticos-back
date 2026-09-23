using MK.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.DTOs.Products
{
    public class ManagmentProductStockDto
    {
        public int Id { get; set; }
        public ProductSize Size { get; set; }
        public int StockQuantity { get; set; }
        public ProductGender StockType { get; set; }
    }
}
