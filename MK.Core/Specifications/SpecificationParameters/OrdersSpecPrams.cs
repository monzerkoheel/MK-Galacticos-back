using MK.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.Specifications.SpecificationParameters
{
    public class OrdersSpecPrams
    {
        public OrderStatus? Status { get; set; }
        public string? Search { get; set; }
        public bool? SortByPrice { get; set; }
        public bool? SortByPriceDes{ get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
