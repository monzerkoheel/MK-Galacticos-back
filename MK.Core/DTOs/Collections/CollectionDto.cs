using MK.Core.HelperModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.DTOs.Collections
{
    public class CollectionDto
    {
        public int Id { get; set; }
        public string Slug { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;
        public string Name { get; set; } = null!;
        public Visibility Status { get; set; }
        public int ProductsCount { get; set; }
    }
}
