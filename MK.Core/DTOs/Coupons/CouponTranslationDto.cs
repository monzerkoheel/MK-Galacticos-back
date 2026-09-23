using MK.Core.HelperModels;

namespace MK.Core.DTOs.Coupons
{
    public class CouponTranslationDto
    {
        public Language Language { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
    }
}
