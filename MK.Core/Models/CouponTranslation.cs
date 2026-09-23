using MK.Core.HelperModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.Models
{
    public class CouponTranslation : ModelBase
    {
        public int CouponId { get; set; }
        public Language Language { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public Coupon Coupon { get; set; } = null!;
    }
}
