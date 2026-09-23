using MK.Core.HelperModels;
using System;
using System.Collections.Generic;

namespace MK.Core.DTOs.ShippingMethods
{
    public class ShippingMethodDetailsDto
    {
        public int Id { get; set; }
        public decimal Price { get; set; }
        public int EstimatedDeliveryDays { get; set; }
        public Visibility Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public ICollection<ShippingMethodTranslationDto> Translations { get; set; } = new List<ShippingMethodTranslationDto>();
    }
}
