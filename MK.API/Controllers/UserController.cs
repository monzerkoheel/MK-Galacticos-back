using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MK.API.Errors;
using MK.Core.Contracts.ServicesContracts;
using MK.Core.DTOs.Account.Authentication;
using MK.Core.DTOs.Account.Information;
using MK.Core.DTOs.Account.Management;
using MK.Core.HelperModels;
using MK.Core.Models;
using MK.Core.Specifications.SpecificationParameters;
using System.Security.Claims;

namespace MK.API.Controllers
{
    [Route("api/management/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserManagementService _userManagementService;

        public UserController(IUserManagementService userManagementService)
        {
            _userManagementService = userManagementService;
        }

        [Authorize(Roles = "Owner,Manager,Admin")]
        [HttpPut("restore-account")]
        public async Task<ActionResult<OperationResultModel>> RestoreUserAccount(EmailDto dto)
            => Ok(await _userManagementService.RestoreAccountAsync(dto));

        [Authorize(Roles = "Owner,Manager,Admin")]
        [HttpPut("ban/{userId}")]
        public async Task<ActionResult<OperationResultModel>> BanUser(string userId, BanUserDto dto)
            => Ok(await _userManagementService.BanUserAsync(userId, dto));

        [Authorize(Roles = "Owner,Manager,Admin")]
        [HttpPut("unban/{userId}")]
        public async Task<ActionResult<OperationResultModel>> UnbanUser(string userId)
            => Ok(await _userManagementService.UnbanUserAsync(userId));

        [Authorize(Roles = "Owner,Manager,Admin")]
        [HttpDelete("delete-user/{userId}")]
        public async Task<ActionResult<OperationResultModel>> DeleteUser(string userId)
            => Ok(await _userManagementService.DeleteUserAsync(userId));

        [Authorize(Roles = "Owner,Manager")]
        [HttpGet("roles")]
        public async Task<ActionResult<IReadOnlyList<string>>> GetRoles()
            => Ok(await _userManagementService.GetRolesAsync());

        [Authorize(Roles = "Owner,Manager")]
        [HttpPost("create-management-user")]
        public async Task<ActionResult<OperationResultModel>> CreateManagementUser(CreateManagementUserDto dto)
            => Ok(await _userManagementService.CreateManagementUserAsync(dto));

        [Authorize(Roles = "Owner,Manager")]
        [HttpPut("change-user-role")]
        public async Task<ActionResult<OperationResultModel>> ChangeUserRole(ChangeUserRoleDto dto)
            => Ok(await _userManagementService.ChangeUserRoleAsync(dto));

        [Authorize(Roles = "Owner,Manager,Admin,Customer Support")]
        [HttpGet("{userId}")]
        public async Task<ActionResult<ApplicationUser?>> GetUserById(string userId)
            => Ok(await _userManagementService.GetCustomerByIdAsync(userId));

        [Authorize(Roles = "Owner,Manager,Admin")]
        [HttpPut("change-user-state")]
        public async Task<ActionResult<OperationResultModel>> ChangeUserState(ChangeUserStateDto dto)
            => Ok(await _userManagementService.ChangeUserStateAsync(dto));

        [Authorize(Roles = "Owner,Manager")]
        [HttpGet("management-users")]
        public async Task<ActionResult<PaginationModel<ManagementUserDto?>>> GetManagementUsers([FromQuery] GetManagementUsersSpecPrams dto)
            => Ok(await _userManagementService.GetUsersAsync(dto));

        [Authorize(Roles = "Owner,Manager")]
        [HttpGet("management-user/{userId}")]
        public async Task<ActionResult<ManagementUserDto?>> GetManagementUserById(string userId)
            => Ok(await _userManagementService.GetUserByIdAsync(userId));

        [Authorize(Roles = "Owner,Manager,Admin,Customer Support")]
        [HttpGet("user-addresses/{userId}")]
        public async Task<ActionResult<IReadOnlyList<UserAddressDto>?>> GetUserAddresses(string userId)
            => Ok(await _userManagementService.GetUserAddressesAsync(userId));

        [Authorize(Roles = "Owner,Manager,Admin,Customer Support")]
        [HttpGet("user-address/{addressId}")]
        public async Task<ActionResult<UserAddressDto>> GetUserAddresse(int addressId)
            => Ok(await _userManagementService.GetUserAddressAsync(addressId));
    }
}
