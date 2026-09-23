using System.ComponentModel.DataAnnotations;

namespace MK.Core.DTOs.Products
{
    public class EditProductImageDto
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public bool IsMain { get; set; }
        [Required]
        public int DisplayOrder { get; set; }
    }
}
