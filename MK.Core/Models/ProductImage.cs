using MK.Core.HelperModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.Models
{
    public class ProductImage : ModelBase
    {
        public int ProductId { get; set; }
        public string ImageUrl { get; set; } = null!;
        public bool IsMain { get; set; }
        public ProductGender Gender { get; set; }
        public int DisplayOrder { get; set; }
        public Product Product { get; set; } = null!;
    }
}
