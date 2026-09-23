using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.DTOs.Products
{
    public class EditProductStocksDto
    {
        public ICollection<int> DeletedStockIds { get; set; } = new List<int>();
        public ICollection<RestockProductStockDto> RestockStock { get; set; }= new List<RestockProductStockDto>();
        public ICollection<AddProductStockDto> AddedStocks { get; set; }= new List<AddProductStockDto>();
    }
}
