using MK.Core.DTOs.Coupons;
using MK.Core.HelperModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.Contracts.ServicesContracts
{
    public interface ICouponService
    {
        Task<OperationResultModel> AddCouponAsync(AddCouponDto dto);
        Task<OperationResultModel> EditCouponAsync(int couponId, EditCouponDto dto);
        Task<OperationResultModel> DeleteCouponAsync(int couponId);
        Task<IReadOnlyList<CouponDto>?> GetAllCouponsAsync();
        Task<IReadOnlyList<CouponDto>?> GetAllActiveCouponsAsync();
        Task<CouponDetailsDto> GetCouponDetailsByIdAsync(int couponId);
        Task<OperationResultModel> AssignUserCouponAsync(int couponId);
        Task<OperationResultModel> AssignCouponToUserAsync(string userId, int couponId);
        Task AssignWelcomeCouponAsync(string userId);
    }
}
