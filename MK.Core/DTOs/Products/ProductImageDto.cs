using Microsoft.AspNetCore.Http;
using MK.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.DTOs.Products
{
    public class ProductImageDto
    {
        public string ImageUrl { get; set; }
        public bool IsMain { get; set; } = false;
        public ProductGender Gender { get; set; }
        public int DisplayOrder { get; set; }
    }
}
