using MK.Core.HelperModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.DTOs.Products
{
    public class ProductCollectionDto
    {
        public int CollectionId { get; set; }
        public Visibility Status { get; set; }
        public string Name { get; set; } = null!;
    }
}
