using MK.Core.Models;
using MK.Core.Specifications.SpecificationParameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Repository.Specifications
{
    public class UserCouponSpecification : BaseSpecification<UserCoupon>
    {
        public UserCouponSpecification(string userId, UserCouponsSpecPrams specParams,bool applyPagination = true)
            : base(x =>x.UserId == userId && 
                  (!specParams.IsUsed.HasValue || x.IsUsed == specParams.IsUsed.Value) &&
                  (!specParams.CouponType.HasValue || x.Coupon.CouponType == specParams.CouponType.Value) &&
                  (!specParams.IsExpired.HasValue || specParams.IsExpired == false || x.ExpirationDate <= DateTime.UtcNow))
        {
            AddInclude(x => x.Coupon);
            AddInclude(x => x.Coupon.Translations);

            if (specParams.SortBy == UserCouponSortBy.AssignedAt)
            {
                if (specParams.SortDescending)
                    AddOrderByDes(x => x.AssignedAt);
                else
                    AddOrderBy(x => x.AssignedAt);
            }
            else if (specParams.SortBy == UserCouponSortBy.ExpirationDate)
            {
                if (specParams.SortDescending)
                    AddOrderByDes(x => x.ExpirationDate);
                else
                    AddOrderBy(x => x.ExpirationDate);
            }

            if (applyPagination)
            {
                ApplyPagination((specParams.PageIndex - 1) * specParams.PageSize, specParams.PageSize);
            }
            
        }
    }
}
