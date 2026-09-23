using MK.Core.HelperModels;
using MK.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.DTOs.Coupons
{
    public class AddCouponDto
    {
        [Required]
        public CouponType CouponType { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public int? RequiredPoints { get; set; }
        public decimal? MinimumOrderAmount { get; set; }
        public decimal? MaximumDiscountAmount { get; set; }
        [Required]
        public Visibility UserVisibility { get; set; }
        [Required]
        public ICollection<AddCouponTranslationDto> Translations { get; set; }  = new List<AddCouponTranslationDto>();
    }
}
