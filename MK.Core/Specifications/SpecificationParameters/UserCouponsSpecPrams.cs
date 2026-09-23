using MK.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.Specifications.SpecificationParameters
{
    public enum UserCouponSortBy
    {
        AssignedAt,
        ExpirationDate
    }
    public class UserCouponsSpecPrams
    {
        public bool? IsUsed { get; set; }
        public CouponType? CouponType { get; set; }
        public bool? IsExpired { get; set; }
        public UserCouponSortBy SortBy { get; set; } = UserCouponSortBy.AssignedAt;
        public bool SortDescending { get; set; } = true;

        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
