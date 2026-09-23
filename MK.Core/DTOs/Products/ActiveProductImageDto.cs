using MK.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.DTOs.Products
{
    public class ActiveProductImageDto
    {
        public string ImageUrl { get; set; } = null!;
        public bool IsMain { get; set; }
        public ProductGender Gender { get; set; }
    }
}
