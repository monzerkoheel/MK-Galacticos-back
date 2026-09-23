using Microsoft.AspNetCore.Http;
using MK.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace MK.Core.DTOs.Products
{
    public class AddProductImageDto
    {
        [Required]
        public IFormFile Image { get; set; }
        public bool IsMain { get; set; } = false;
        [Required]
        public ProductGender Gender { get; set; }
        [Required]
        public int DisplayOrder { get; set; }
    }
}
