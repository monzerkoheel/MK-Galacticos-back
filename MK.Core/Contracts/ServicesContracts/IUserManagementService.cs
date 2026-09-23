using MK.Core.DTOs.Account.Authentication;
using MK.Core.DTOs.Account.Information;
using MK.Core.DTOs.Account.Management;
using MK.Core.HelperModels;
using MK.Core.Models;
using MK.Core.Specifications.SpecificationParameters;


namespace MK.Core.Contracts.ServicesContracts
{
    public interface IUserManagementService
    {
        Task<OperationResultModel> RestoreAccountAsync(EmailDto dto);
        Task<OperationResultModel> BanUserAsync(string userId, BanUserDto dto);
        Task<OperationResultModel> UnbanUserAsync(string userId);
        Task<OperationResultModel> DeleteUserAsync(string userId);
        Task<IReadOnlyList<string>> GetRolesAsync();
        Task<OperationResultModel> CreateManagementUserAsync(CreateManagementUserDto dto);
        Task<OperationResultModel> ChangeUserRoleAsync(ChangeUserRoleDto dto);
        Task<OperationResultModel> ChangeUserStateAsync(ChangeUserStateDto dto);
        Task<PaginationModel<ManagementUserDto?>> GetUsersAsync(GetManagementUsersSpecPrams dto);
        Task<ManagementUserDto> GetUserByIdAsync(string userId);
        Task<IReadOnlyList<UserAddressDto>?> GetUserAddressesAsync(string userId);
        Task<UserAddressDto> GetUserAddressAsync(int addressId);


        Task<ApplicationUser?> GetCustomerByIdAsync(string userId);
    }
}
