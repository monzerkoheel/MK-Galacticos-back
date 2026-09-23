using MK.Core.HelperModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.Models
{
    public class ProductPrint : ModelBase
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public int Number { get; set; }

        public Product Product { get; set; }
    }
}
