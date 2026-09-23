using MK.Core.HelperModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.Models
{
    public class ProductTranslation : ModelBase
    {
        public int ProductId { get; set; }
        public Language Language { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}
