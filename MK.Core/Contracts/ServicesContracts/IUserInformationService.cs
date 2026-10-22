using MK.Core.DTOs.Account.Information;
using MK.Core.DTOs.Orders;
using MK.Core.DTOs.Products;
using MK.Core.HelperModels;
using MK.Core.Models;
using MK.Core.Specifications.SpecificationParameters;

namespace MK.Core.Contracts.ServicesContracts
{
    public interface IUserInformationService
    {
        Task<IReadOnlyList<CountryDto>> GetAvailableCountriesAsync();
        Task<IReadOnlyList<GovernorateDto>> GetCountryGovernoratesAsync(int countryId);
        Task<OperationResultModel> CreateUserAddress(CreateUserAddressDto dto);
        Task<IReadOnlyList<UserAddressDto>?> GetUserAddressesAsync();
        Task<UserAddressDto> GetUserAddressAsync(int addressId);
        Task<UserAddressDto> EditUserAddressAsync(EditUserAddressDto dto);
        Task<OperationResultModel> DeleteUserAddressAsync(int addressId);
        Task<UserProfileDto> EditUserInfoAsync(EditUserInformationsDto dto);
        Task<UserProfileDto> UploadUserImageAsync(UploadUserImageDto dto);
        Task<UserProfileDto> DeleteUserImageAsync();
        Task<UserProfileDto> GetProfileAsync();
        Task<SimpleUserDataDto> GetSimpleUserData();
        Task<PaginationModel<LoyaltyTransactionDto>> GetAllUserTransaction(LoyaltyTransactionSpecPrams specPrams);
        Task<PaginationModel<UserCouponDto>> GetUserCouponsAsync(UserCouponsSpecPrams specParams);
        Task<IReadOnlyList<ActiveProductsDto>> GetUserWishList();
        Task<IReadOnlyList<CartItemDto>> GetUserCart();
        Task<PaginationModel<OrderDto>> GetUserOrdersAsync(PaginationRequest dto);
    }
}
