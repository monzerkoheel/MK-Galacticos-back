using MK.Core.HelperModels;
using System.Collections.Generic;

namespace MK.Core.DTOs.ShippingMethods
{
    public class UpdateShippingMethodDto
    {
        public decimal? Price { get; set; }
        public int? EstimatedDeliveryDays { get; set; }
        public Visibility? Status { get; set; }
        public ICollection<UpdateShippingMethodTranslationDto>? Translations { get; set; }
    }
}
