using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MK.Core.Contracts.RepositoriesContracts;
using MK.Core.Contracts.ServicesContracts;
using MK.Core.DTOs.Account.Authentication;
using MK.Core.DTOs.Account.Information;
using MK.Core.DTOs.Account.Management;
using MK.Core.HelperModels;
using MK.Core.HelperModels.Exceptions;
using MK.Core.Localization.Resources;
using MK.Core.Models;
using MK.Repository.Specifications;
using MK.Core.Specifications.SpecificationParameters;
using MK.Repository.Data;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace MK.Service.Services
{
    public class UserManagementService : IUserManagementService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly MKGalacticosDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IRequestContext _requestContext;
        public UserManagementService(IConfiguration configuration,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            MKGalacticosDbContext dbContext,
            IMapper mapper,
            IRequestContext requestContext)
        {
            _configuration = configuration;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
            _currentUser = currentUserService;
            _dbContext = dbContext;
            _mapper = mapper;
            _requestContext = requestContext;
        }
        public async Task<OperationResultModel> RestoreAccountAsync(EmailDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user is null)
                throw new BadRequestException(Messages.AccountNotFound);

            if (user.Status != UserStatus.Deleted)
                throw new BadRequestException(Messages.AccountNotDeleted);

            user.Status = UserStatus.Active;
            user.UpdatedAt = DateTime.UtcNow;
            user.EmailConfirmed = false;

            var check = await _userManager.UpdateAsync(user);

            if (!check.Succeeded)
                throw new Exception(Messages.SomethingWentWrong);

            var userSpec = new BaseSpecification<DeletedAccount>(u => u.UserId == user.Id);

            var deletedUser = await _unitOfWork.Repository<DeletedAccount>().GetFirstOrDefaultAsync(userSpec);

            if (deletedUser is not null)
            {
                _unitOfWork.Repository<DeletedAccount>().Delete(deletedUser);

                if (await _unitOfWork.CompleteAsync() < 1)
                    throw new Exception(Messages.SomethingWentWrong);
            }

            return new OperationResultModel
            {
                IsSuccess = true,
                Message = Messages.AccountRestoredSuccessfully
            };
        }

        public async Task<OperationResultModel> BanUserAsync(string userId, BanUserDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user is null)
                throw new BadRequestException(Messages.AccountNotFound);

            var userrole = (await _userManager.GetRolesAsync(user)).FirstOrDefault();

            if (userrole != "Customer")
                throw new BadRequestException(Messages.CannotBanManagementUser);

            if (user.Status == UserStatus.Deleted)
                throw new BadRequestException(Messages.AccountDeleted);

            if (user.Status == UserStatus.Banned)
                throw new BadRequestException(Messages.AccountAlreadyBanned);

            user.Status = UserStatus.Banned;
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                throw new Exception(Messages.SomethingWentWrong);

            await _unitOfWork.Repository<BannedUser>().AddAsync(
                new BannedUser
                {
                    UserId = user.Id,
                    BannedAt = DateTime.UtcNow,
                    BannedUntil = dto.BannedUntil,
                    BanReason = dto.BanReason
                });

            if (await _unitOfWork.CompleteAsync() < 1)
                throw new Exception(Messages.SomethingWentWrong);

            return new OperationResultModel
            {
                IsSuccess = true,
                Message = Messages.UserBannedSuccessfully
            };
        }

        public async Task<OperationResultModel> UnbanUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user is null)
                throw new BadRequestException(Messages.AccountNotFound);

            if (user.Status != UserStatus.Banned)
                throw new BadRequestException(Messages.AccountNotBanned);

            user.Status = UserStatus.Active;
            user.UpdatedAt = DateTime.UtcNow;
            user.EmailConfirmed = false;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                throw new Exception(Messages.SomethingWentWrong);

            var bannedUserSpec = new BaseSpecification<BannedUser>(x => x.UserId == user.Id);

            var bannedUser = await _unitOfWork.Repository<BannedUser>().GetFirstOrDefaultAsync(bannedUserSpec);

            if (bannedUser is not null)
            {
                _unitOfWork.Repository<BannedUser>().Delete(bannedUser);

                if (await _unitOfWork.CompleteAsync() < 1)
                    throw new Exception(Messages.SomethingWentWrong);
            }

            return new OperationResultModel
            {
                IsSuccess = true,
                Message = Messages.UserUnbannedSuccessfully
            };
        }

        public async Task<OperationResultModel> DeleteUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user is null)
                throw new BadRequestException(Messages.AccountNotFound);

            await EnsureCanManageUserAsync(_currentUser.Role!, user);

            if (user.Status == UserStatus.Deleted)
                throw new BadRequestException(Messages.AccountAlreadyDeleted);

            await _unitOfWork.Repository<DeletedAccount>().AddAsync(new DeletedAccount
            {
                UserId = user.Id,
                DeletedAt = DateTime.UtcNow,
                DeleteDate = DateTime.UtcNow.AddMonths(3)
            });

            user.UpdatedAt = DateTime.UtcNow;
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            user.Status = UserStatus.Deleted;

            if (await _unitOfWork.CompleteAsync() < 1)
                throw new Exception(Messages.SomethingWentWrong);

            var check = await _userManager.UpdateAsync(user);

            if (!check.Succeeded)
                throw new Exception(Messages.SomethingWentWrong);

            return new OperationResultModel
            {
                IsSuccess = true,
                Message = Messages.UserDeletedSuccessfully
            };


        }

        public async Task<IReadOnlyList<string>> GetRolesAsync()
            => await _roleManager.Roles.Where(x => x.Name != "Owner").Select(x => x.Name!).ToListAsync();

        public async Task<OperationResultModel> CreateManagementUserAsync(CreateManagementUserDto dto)
        {
            var currentUserRole = _currentUser.Role;

            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user is not null)
                throw new BadRequestException(Messages.AccountAlreadyExists);

            if (!CanManageUser(currentUserRole!,dto.Role))
                throw new ForbiddenException(Messages.InsufficientPermissions);


            var newUser = new ApplicationUser
            {
                FullName = dto.FullName.Trim(),
                Email = dto.Email.Trim(),
                UserName = dto.Email.Trim(),
                EmailConfirmed = true,
                PhoneNumber = dto.PhoneNumber?.Trim(),
                Status = UserStatus.Active,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(newUser, dto.Password);

            if (!result.Succeeded)
                throw new BadRequestException(Messages.SomethingWentWrong);

            var roleResult = await _userManager.AddToRoleAsync(newUser, dto.Role);

            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(newUser);
                throw new BadRequestException(Messages.SomethingWentWrong);
            }

            var newValues = new Dictionary<string, object?>
            {
                ["Id"] = newUser.Id,
                ["FullName"] = newUser.FullName,
                ["Email"] = newUser.Email,
                ["PhoneNumber"] = newUser.PhoneNumber,
                ["Role"] = dto.Role
            };

            var auditLog = new AuditLog
            {
                UserId = _currentUser.UserId,
                Action = AuditAction.Create,
                ActorType = AuditActorType.Management,
                EntityName = nameof(ApplicationUser),
                EntityId = newUser.Id,
                NewValues = JsonSerializer.Serialize(newValues, AuditJsonOptions),
                Description = "Create Management User",
                IpAddress = _requestContext.IpAddress,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<AuditLog>().AddAsync(auditLog);
            await _unitOfWork.CompleteAsync();


            return new OperationResultModel
            {
                IsSuccess = true,
                Message = Messages.UserCreatedSuccessfully
            };
        }

        public async Task<OperationResultModel> ChangeUserRoleAsync(ChangeUserRoleDto dto)
        {
            var currentUserRole = _currentUser.Role;

            var user = await _userManager.FindByIdAsync(dto.UserId);

            if (user is null)
                throw new BadRequestException(Messages.AccountAlreadyExists);

            if (!CanManageUser(currentUserRole!, dto.NewRole))
                throw new ForbiddenException(Messages.InsufficientPermissions);

            var currentUserRoles = await _userManager.GetRolesAsync(user);

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var roleResult = await _userManager.AddToRoleAsync(user, dto.NewRole);

                if (!roleResult.Succeeded)
                    throw new BadRequestException(Messages.SomethingWentWrong);

                var removeRoleResult = await _userManager.RemoveFromRolesAsync(user, currentUserRoles);

                if (!removeRoleResult.Succeeded)
                    throw new BadRequestException(Messages.SomethingWentWrong);

                await _unitOfWork.CommitTransactionAsync();

                var oldValues = new Dictionary<string, object?>
                {
                    ["Role"] = currentUserRole
                };

                var newValues = new Dictionary<string, object?>
                {
                    ["Role"] = dto.NewRole
                };

                var auditLog = new AuditLog
                {
                    UserId = _currentUser.UserId,
                    Action = AuditAction.Create,
                    ActorType = AuditActorType.Management,
                    EntityName = nameof(ApplicationUser),
                    EntityId = user.Id,
                    OldValues = JsonSerializer.Serialize(oldValues, AuditJsonOptions),
                    NewValues = JsonSerializer.Serialize(newValues, AuditJsonOptions),
                    Description = "Change Management User Role",
                    IpAddress = _requestContext.IpAddress,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Repository<AuditLog>().AddAsync(auditLog);
                await _unitOfWork.CompleteAsync();

                return new OperationResultModel
                {
                    IsSuccess = true,
                    Message = Messages.UserRoleChangedSuccessfully
                };
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<OperationResultModel> ChangeUserStateAsync(ChangeUserStateDto dto)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user is null)
                throw new BadRequestException(Messages.AccountNotFound);

            var currentUserRole = _currentUser.Role;
            await EnsureCanManageUserAsync(currentUserRole!, user);

            if (dto.UserNewState == UserNewState.Inactive)
            {
                user.Status = UserStatus.Inactive;
                user.EmailConfirmed = false;
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;
            }
            else
            {
                user.Status = UserStatus.Active;
                user.EmailConfirmed = true;
            }

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                throw new BadRequestException(Messages.SomethingWentWrong);

            return new OperationResultModel
            {
                IsSuccess = true,
                Message = Messages.UserStateChangedSuccessfully
            };
        }

        public async Task<PaginationModel<ManagementUserDto?>> GetUsersAsync(GetManagementUsersSpecPrams dto)
        {
            var query = from user in _userManager.Users
                        join userRole in _dbContext.Set<IdentityUserRole<string>>()
                            on user.Id equals userRole.UserId
                        join role in _roleManager.Roles
                            on userRole.RoleId equals role.Id
                        where role.Name != "Customer"
                        select new ManagementUserDto
                        {
                            Id = user.Id,
                            ImageUrl = user.ProfileImageUrl,
                            FullName = user.FullName,
                            Email = user.Email,
                            PhoneNumber = user.PhoneNumber,
                            Role = role.Name,
                            Status = user.Status,
                            LastLoginAt = user.LastLoginAt,
                            CreatedAt = user.CreatedAt
                        };

            var data = query.Where(x => (dto.Search == null || x.FullName.Contains(dto.Search) 
                                                            || x.Email.Contains(dto.Search)) &&
                                        (dto.Role == null || x.Role == dto.Role) &&
                                        (dto.Status == null || x.Status == dto.Status));
            var totalCount = await data.CountAsync();
            data = data.Skip((dto.PageIndex - 1) * dto.PageSize).Take(dto.PageSize);
            var result = new PaginationModel<ManagementUserDto>(
                pageIndex: dto.PageIndex,
                pageSize: dto.PageSize,
                totalCount: totalCount,
                data: await data.ToListAsync()
            );
            return result;
        }

        public async Task<ManagementUserDto> GetUserByIdAsync(string userId)
        {
            var result = from user in _userManager.Users
                        join userRole in _dbContext.Set<IdentityUserRole<string>>()
                            on user.Id equals userRole.UserId
                        join role in _roleManager.Roles
                            on userRole.RoleId equals role.Id
                        where user.Id == userId
                        select new ManagementUserDto
                        {
                            Id = user.Id,
                            ImageUrl = user.ProfileImageUrl,
                            FullName = user.FullName,
                            Email = user.Email,
                            PhoneNumber = user.PhoneNumber,
                            Role = role.Name,
                            Status = user.Status,
                            LastLoginAt = user.LastLoginAt,
                            CreatedAt = user.CreatedAt
                        };
            return result.FirstOrDefault();
        }

        public async Task<IReadOnlyList<UserAddressDto>?> GetUserAddressesAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                throw new BadRequestException(Messages.AccountNotFound);

            await EnsureCanManageUserAsync(_currentUser.Role!, user);

            var userAddresses = await _unitOfWork.Repository<UserAddress>()
                .ListAsync(new BaseSpecification<UserAddress>(ua => ua.UserId == userId));

            return _mapper.Map<IReadOnlyList<UserAddressDto>>(userAddresses);
        }

        public async Task<UserAddressDto> GetUserAddressAsync(int addressId)
        {
            var userAddress = await _unitOfWork.Repository<UserAddress>().GetByIdAsync(addressId);
            if (userAddress == null)
                throw new NotFoundException(Messages.AddressNotFound);

            return _mapper.Map<UserAddressDto>(userAddress);
        }



        public async Task<ApplicationUser?> GetCustomerByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                throw new BadRequestException(Messages.AccountNotFound);

            var currentUserRole = _currentUser.Role;
            await CanGetUser(currentUserRole!, user);

            return user;
        }



        #region Helping Methods
        private async Task EnsureCanManageUserAsync(string currentUserRole, ApplicationUser targetUser)
        {
            var targetUserRole = (await _userManager.GetRolesAsync(targetUser)).FirstOrDefault();

            var canManage = CanManageUser(currentUserRole, targetUserRole!);

            if (!canManage)
                throw new ForbiddenException(Messages.InsufficientPermissions);
        }

        private bool CanManageUser(string currentUserRole, string targetUserRole)
        {
            var canManage = currentUserRole switch
            {
                "Owner" => true,
                "Manager" => targetUserRole is "Admin" or "Editor" or "Customer Support" or "Customer",
                "Admin" => targetUserRole == "Customer",
                _ => false
            };
            return canManage;
        }

        private async Task CanGetUser(string currentUserRole, ApplicationUser targetUser)
        {
            var targetUserRole = (await _userManager.GetRolesAsync(targetUser)).FirstOrDefault();

            var canGet = currentUserRole switch
            {
                "Owner" => true,
                "Manager" => targetUserRole is "Admin" or "Editor" or "Customer Support" or "Customer",
                "Admin" => targetUserRole == "Customer",
                "Customer Support" => targetUserRole == "Customer",
                _ => false
            };

            if (!canGet)
                throw new ForbiddenException(Messages.InsufficientPermissions);
        }

        private static readonly JsonSerializerOptions AuditJsonOptions = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Converters =
                {
                    new JsonStringEnumConverter()
                }
        };

        #endregion
    }
}
