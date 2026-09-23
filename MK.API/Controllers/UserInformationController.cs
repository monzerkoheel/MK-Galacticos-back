using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MK.Core.Contracts.ServicesContracts;
using MK.Core.DTOs.Account.Information;
using MK.Core.DTOs.Products;
using MK.Core.HelperModels;
using MK.Core.Models;
using MK.Core.Specifications.SpecificationParameters;

namespace MK.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserInformationController : ControllerBase
    {
        private readonly IUserInformationService _userInformationService;
        public UserInformationController(IUserInformationService userInformationService)
        {
            _userInformationService = userInformationService;
        }

        [HttpGet("countries")]
        public async Task<ActionResult<IReadOnlyList<CountryDto>>> GetAvailableCountries()
            => Ok(await _userInformationService.GetAvailableCountriesAsync());

        [HttpGet("country-governorates/{countryId}")]
        public async Task<ActionResult<IReadOnlyList<GovernorateDto>>> GetCountryGovernorates(int countryId)
            => Ok(await _userInformationService.GetCountryGovernoratesAsync(countryId));

        [Authorize]
        [HttpPost("create-address")]
        public async Task<ActionResult<OperationResultModel>> CreateUserAddress(CreateUserAddressDto dto)
            => Ok(await _userInformationService.CreateUserAddress(dto));

        [Authorize]
        [HttpGet("user-addresses")]
        public async Task<ActionResult<IReadOnlyList<UserAddressDto>?>> GetUserAddresses()
            => Ok(await _userInformationService.GetUserAddressesAsync());

        [Authorize]
        [HttpGet("user-address/{addressId}")]
        public async Task<ActionResult<UserAddressDto>> GetUserAddresse(int addressId)
            => Ok(await _userInformationService.GetUserAddressAsync(addressId));

        [Authorize]
        [HttpPut("edit-address")]
        public async Task<ActionResult<UserAddressDto>> EditUserAddress(EditUserAddressDto dto)
            => Ok(await _userInformationService.EditUserAddressAsync(dto));

        [Authorize]
        [HttpDelete("delete-address/{addressId}")]
        public async Task<ActionResult<OperationResultModel>> DeleteUserAddress(int addressId)
            => Ok(await _userInformationService.DeleteUserAddressAsync(addressId));

        [Authorize]
        [HttpGet("profile")]
        public async Task<ActionResult<UserProfileDto>> GetProfile()
            => Ok(await _userInformationService.GetProfileAsync());

        [Authorize]
        [HttpPut("edit-profile")]
        public async Task<ActionResult<UserProfileDto>> EditUserProfile(EditUserInformationsDto dto)
            => Ok(await _userInformationService.EditUserInfoAsync(dto));

        [Authorize]
        [HttpPut("upload-user-image")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<UserProfileDto>> UploadUserImage([FromForm] UploadUserImageDto dto)
            => Ok(await _userInformationService.UploadUserImageAsync(dto));

        [Authorize]
        [HttpDelete("delete-user-image")]
        public async Task<ActionResult<UserProfileDto>> DeleteUserImage()
            => Ok(await _userInformationService.DeleteUserImageAsync());


        [Authorize]
        [HttpGet("simple-user")]
        public async Task<ActionResult<SimpleUserDataDto>> GetSimpleUserData()
            => Ok(await _userInformationService.GetSimpleUserData());

        [Authorize]
        [HttpGet("user-loyalty-points-transactions")]
        public async Task<ActionResult<PaginationModel<LoyaltyTransactionDto>>> GetAllUserTransaction([FromQuery] LoyaltyTransactionSpecPrams specPrams)
            => Ok(await _userInformationService.GetAllUserTransaction(specPrams));

        [Authorize]
        [HttpGet("user-coupons")]
        public async Task<ActionResult<PaginationModel<UserCouponDto>>> GetUserCoupons([FromQuery] UserCouponsSpecPrams specParams)
            => Ok(await _userInformationService.GetUserCouponsAsync(specParams));

        [Authorize]
        [HttpGet("user-wish-list")]
        public async Task<ActionResult<IReadOnlyList<ActiveProductsDto>>> GetUserWishList()
            => Ok(await _userInformationService.GetUserWishList());

        [Authorize]
        [HttpGet("user-cart")]
        public async Task<ActionResult<IReadOnlyList<CartItemDto>>> GetUserCart()
            => Ok(await _userInformationService.GetUserCart());
    }
}
