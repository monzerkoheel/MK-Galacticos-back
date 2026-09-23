using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.DTOs.Products
{
    public class EditProductPrintsDto
    {
        public ICollection<int> DeletedPrintsIds { get; set; } = new List<int>();
        public ICollection<AddProductPrintDto> AddedPrints { get; set; } = new List<AddProductPrintDto>();
    }
}
