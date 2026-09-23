using MK.Core.HelperModels;
using System.ComponentModel.DataAnnotations;


namespace MK.Core.DTOs.Products
{
    public class AddProductTranslationDto
    {
        [Required]
        public Language Language { get; set; }
        [Required]
        public string Name { get; set; } = null!;
        [Required]
        public string Description { get; set; } = null!;
    }
}
