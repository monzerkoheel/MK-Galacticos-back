using MK.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.DTOs.Account.Information
{
    public class UserCouponDto
    {
        public int Id { get; set; }
        public int CouponId { get; set; }
        public string Code { get; set; } = null!;
        public CouponAcquisitionType AcquisitionType { get; set; }
        public DateTime AssignedAt { get; set; }
        public DateTime ExpirationDate { get; set; }
        public bool IsUsed { get; set; }
        public DateTime? UsedAt { get; set; }

        public CouponType CouponType { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal? DiscountAmount { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public decimal? MinimumOrderAmount { get; set; }
        public decimal? MaximumDiscountAmount { get; set; }
    }
}
