using MK.Core.HelperModels;

namespace MK.Core.Models
{
    public enum CouponType
    {
        Percentage,
        FixedAmount,
        FreeShipping,
        FreeShippingUpgrade
    }
    public class Coupon : ModelBase
    {
        public CouponType CouponType { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public int? RequiredPoints { get; set; }
        public decimal? MinimumOrderAmount { get; set; }
        public decimal? MaximumDiscountAmount { get; set; }
        public Visibility UserVisibility { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ICollection<CouponTranslation> Translations { get; set; } = new List<CouponTranslation>();
        public ICollection<UserCoupon> UserCoupons { get; set; } = new List<UserCoupon>();
    }
}
