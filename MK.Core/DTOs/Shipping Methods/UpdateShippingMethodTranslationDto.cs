using MK.Core.HelperModels;
using System.ComponentModel.DataAnnotations;

namespace MK.Core.DTOs.ShippingMethods
{
    public class UpdateShippingMethodTranslationDto
    {
        [Required]
        public Language Language { get; set; }
        [Required]
        public string Name { get; set; }
    }
}
