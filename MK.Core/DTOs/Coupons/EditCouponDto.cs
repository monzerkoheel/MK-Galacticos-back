using MK.Core.HelperModels;
using MK.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.DTOs.Coupons
{
    public class EditCouponDto
    {
        public CouponType? CouponType { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public int? RequiredPoints { get; set; }
        public decimal? MinimumOrderAmount { get; set; }
        public decimal? MaximumDiscountAmount { get; set; }
        public Visibility? UserVisibility { get; set; }
        public ICollection<EditCouponTranslationDto>? Translations { get; set; }
    }
}
