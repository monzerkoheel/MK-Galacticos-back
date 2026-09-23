using MK.Core.HelperModels;

namespace MK.Core.DTOs.ShippingMethods
{
    public class ShippingMethodTranslationDto
    {
        public Language Language { get; set; }
        public string Name { get; set; } = null!;
    }
}
