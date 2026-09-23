using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MK.Core.Contracts.ServicesContracts;
using MK.Core.DTOs.Coupons;
using MK.Core.HelperModels;
using MK.Core.Localization.Resources;
using System.Data;

namespace MK.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CouponController : ControllerBase
    {
        private readonly ICouponService _couponService;

        public CouponController(ICouponService couponService)
        {
            _couponService = couponService;
        }

        [Authorize(Roles = "Owner,Manager,Admin")]
        [HttpPost("create-coupon")]
        public async Task<ActionResult<OperationResultModel>> AddCoupon(AddCouponDto dto)
            => Ok(await _couponService.AddCouponAsync(dto));

        [Authorize(Roles = "Owner,Manager,Admin")]
        [HttpPut("{couponId:int}")]
        public async Task<IActionResult> EditCoupon(int couponId, EditCouponDto dto)
            => Ok(await _couponService.EditCouponAsync(couponId, dto));

        [Authorize(Roles = "Owner,Manager,Admin")]
        [HttpDelete("{couponId:int}")]
        public async Task<IActionResult> DeleteCoupon(int couponId)
            => Ok(await _couponService.DeleteCouponAsync(couponId));

        [Authorize(Roles = "Owner,Manager,Admin,Editor,Customer Support")]
        [HttpGet("all-coupons")]
        public async Task<ActionResult<IReadOnlyList<CouponDto>?>> GetAllCoupons()
            => Ok(await _couponService.GetAllCouponsAsync());

        [HttpGet("active-coupons")]
        public async Task<ActionResult<IReadOnlyList<CouponDto>?>> GetAllActiveCoupons()
            => Ok(await _couponService.GetAllActiveCouponsAsync());

        [Authorize(Roles = "Owner,Manager,Admin,Editor,Customer Support")]
        [HttpGet("{couponId:int}")]
        public async Task<ActionResult<CouponDetailsDto>> GetCouponDetailsById(int couponId)
            => Ok(await _couponService.GetCouponDetailsByIdAsync(couponId));

        [Authorize(Roles = "Owner,Manager,Admin,Customer Support")]
        [HttpPost("{couponId:int}/assign-to-user/{userId}")]
        public async Task<ActionResult<OperationResultModel>> AssignCouponToUser(string userId, int couponId)
            => Ok(await _couponService.AssignCouponToUserAsync(userId, couponId));

        [Authorize]
        [HttpPost("{couponId:int}/assign-user-coupon")]
        public async Task<ActionResult<OperationResultModel>> AssignUserCoupon(int couponId)
            => Ok(await _couponService.AssignUserCouponAsync(couponId));

    }
}
