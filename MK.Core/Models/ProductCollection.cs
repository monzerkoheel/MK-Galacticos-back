using MK.Core.HelperModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.Models
{
    public class ProductCollection : ModelBase
    {
        public int ProductId { get; set; }

        public int CollectionId { get; set; }
        public Product Product { get; set; } = null!;

        public Collection Collection { get; set; } = null!;
    }
}
