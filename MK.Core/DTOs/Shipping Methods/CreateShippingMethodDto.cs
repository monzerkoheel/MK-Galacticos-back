using MK.Core.HelperModels;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MK.Core.DTOs.ShippingMethods
{
    public class CreateShippingMethodDto
    {
        [Required]
        public decimal Price { get; set; }
        [Required]
        public int EstimatedDeliveryDays { get; set; }
        public Visibility Status { get; set; } = Visibility.Inactive;
        [Required]
        public ICollection<CreateShippingMethodTranslationDto> Translations { get; set; } = new List<CreateShippingMethodTranslationDto>();
    }
}
