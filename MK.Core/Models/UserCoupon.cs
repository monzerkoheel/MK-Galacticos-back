using MK.Core.HelperModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.Models
{
    public enum CouponAcquisitionType
    {
        Gift,
        Points
    }
    public class UserCoupon : ModelBase
    {
        public string UserId { get; set; } = null!;
        public int CouponId { get; set; }
        public string Code { get; set; } = null!;
        public CouponAcquisitionType AcquisitionType { get; set; }

        public DateTime AssignedAt { get; set; }
        public DateTime ExpirationDate { get; set; }

        public bool IsUsed { get; set; }
        public DateTime? UsedAt { get; set; }


        public ApplicationUser User { get; set; } = null!;
        public Coupon Coupon { get; set; } = null!;
        public Order? Order { get; set; }
    }
}
