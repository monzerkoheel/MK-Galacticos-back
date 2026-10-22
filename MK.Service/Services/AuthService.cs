using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MK.Core.Contracts.RepositoriesContracts;
using MK.Core.Contracts.ServicesContracts;
using MK.Core.DTOs.Account.Authentication;
using MK.Core.HelperModels;
using MK.Core.HelperModels.Exceptions;
using MK.Core.Localization.Resources;
using MK.Core.Models;
using MK.Repository.Specifications;
using MK.Service.HelperServices.EmailSevrices.Templates;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MK.Service.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IMemoryCache _memoryCache;
        private readonly IEmailService _emailService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRequestContext _requestContext;
        private readonly ICouponService _couponService;

        public AuthService(IConfiguration configuration, 
            UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager, 
            IMemoryCache memoryCache, 
            IEmailService emailService, 
            IUnitOfWork unitOfWork,
            IHttpContextAccessor httpContextAccessor,
            IRequestContext requestContext,
            ICouponService couponService)
        {
            _configuration = configuration;
            _userManager = userManager;
            _signInManager = signInManager;
            _memoryCache = memoryCache;
            _couponService = couponService;
            _requestContext = requestContext;
            _httpContextAccessor = httpContextAccessor;
            _unitOfWork = unitOfWork;
            _emailService = emailService;

        }
        public async Task<OperationResultModel> RegisterAsync(RegisterDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user is not null)
            {
                throw new BadRequestException(Messages.AccountAlreadyExists);
            }
            var fullName = dto.FirstName;
            if (dto.LastName != null) 
            {
                fullName = fullName + " " + dto.LastName;
            }
            var newUser = new ApplicationUser
            {
                FullName = fullName,
                Email = dto.Email.Trim(),
                EmailConfirmed = false,
                UserName = dto.Email.Trim(),
                Status = UserStatus.Inactive,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(newUser, dto.Password);
            if (!result.Succeeded)
                throw new Exception(Messages.SomethingWentWrong);

            var roleResult = await _userManager.AddToRoleAsync(newUser, "Customer");

            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(newUser);
                throw new Exception(Messages.SomethingWentWrong);
            }

            await SendOtpAsync(dto.Email);

            var newValues = new Dictionary<string, object?>
            {
                ["Id"] = newUser.Id,
                ["FullName"] = newUser.FullName,
                ["Email"] = newUser.Email,
                ["PhoneNumber"] = newUser.PhoneNumber,
                ["Status"] = newUser.Status
            };

            var auditLog = new AuditLog
            {
                UserId = newUser.Id,
                Action = AuditAction.Create,
                ActorType = AuditActorType.User,
                EntityName = nameof(ApplicationUser),
                EntityId = newUser.Id,
                NewValues = JsonSerializer.Serialize(newValues,AuditJsonOptions),
                Description = "User registered",
                IpAddress = _requestContext.IpAddress,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<AuditLog>().AddAsync(auditLog);
            await _unitOfWork.CompleteAsync();

            return new OperationResultModel
            {
                IsSuccess = true,
                Message = Messages.EmailVerificationRequired
            };
        }

        public async Task<OperationResultModel> ResendOtpAsync(EmailDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user is null)
                throw new BadRequestException(Messages.AccountNotFound);

            if (user.EmailConfirmed)
                throw new BadRequestException(Messages.EmailAlreadyVerified);


            var otpKey = $"OTP_{dto.Email}";
            var lastSentKey = $"OTP_LAST_SENT_{dto.Email}";
            var resendCountKey = $"OTP_RESEND_COUNT_{dto.Email}";
            var blockKey = $"OTP_RESEND_BLOCK_{dto.Email}";

            if (_memoryCache.TryGetValue(blockKey, out _))
                throw new BadRequestException(Messages.OtpResendBlocked);

            if (_memoryCache.TryGetValue(lastSentKey, out DateTime lastSentAt))
            {
                if (DateTime.UtcNow - lastSentAt < TimeSpan.FromMinutes(1))
                    throw new BadRequestException(Messages.OtpResendCooldown);
            }
            var resendCount = 0;

            if (_memoryCache.TryGetValue(resendCountKey, out int count))
                resendCount = count;
            if (resendCount >= 5)
            {
                _memoryCache.Set(blockKey, true, TimeSpan.FromMinutes(30));
                throw new BadRequestException(Messages.OtpResendBlocked);
            }

            var otpCode = GenerateOtp();

            _memoryCache.Set(otpKey, otpCode, TimeSpan.FromMinutes(5));
            _memoryCache.Set(lastSentKey, DateTime.UtcNow, TimeSpan.FromMinutes(30));

            resendCount++;
            _memoryCache.Set(resendCountKey, resendCount, TimeSpan.FromMinutes(30));

            await SendVerifyEmailAsync(dto.Email, otpCode);

            return new OperationResultModel
            {
                IsSuccess = true,
                Message = Messages.OtpResentSuccessfully
            };
        }

        public async Task<OperationResultModel> ChangeEmailAsync(ChangeEmailDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.CurrentEmail);

            if (user is null)
                throw new BadRequestException(Messages.AccountNotFound);

            if (user.Status == UserStatus.Banned || user.Status == UserStatus.Deleted)
                throw new BadRequestException(Messages.SomethingWentWrong);

            if (user.EmailConfirmed || user.Status == UserStatus.Active)
                throw new BadRequestException(Messages.CannotChangeEmailAfterActivation);

            var emailExists = await _userManager.FindByEmailAsync(dto.NewEmail);

            if (emailExists is not null)
                throw new BadRequestException(Messages.EmailAlreadyExists);

            var token = await _userManager.GenerateChangeEmailTokenAsync(user, dto.NewEmail);

            var result = await _userManager.ChangeEmailAsync(user, dto.NewEmail, token);

            if (!result.Succeeded)
                throw new BadRequestException(Messages.SomethingWentWrong);

            user.UserName = dto.NewEmail;
            user.EmailConfirmed = false;
            user.UpdatedAt = DateTime.UtcNow;

            await _userManager.UpdateAsync(user);

            var oldOtpKey = $"OTP_{dto.CurrentEmail}";
            var blockKey = $"OTP_RESEND_BLOCK_{dto.CurrentEmail}";
            _memoryCache.Remove(oldOtpKey);
            _memoryCache.Remove(blockKey);

            await SendOtpAsync(dto.NewEmail);

            return new OperationResultModel
            {
                IsSuccess = true,
                Message = Messages.EmailChangedSuccessfully
            };
        }

        public async Task<OperationResultModel> ConfirmEmailAsync(ConfirmEmailDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user is null)
                throw new BadRequestException(Messages.AccountNotFound);

            if (user.EmailConfirmed)
                throw new BadRequestException(Messages.EmailAlreadyVerified);

            var cacheKey = $"OTP_{dto.Email}";

            if (!_memoryCache.TryGetValue(cacheKey, out string? storedOtp))
                throw new BadRequestException(Messages.OtpExpired);

            if (storedOtp != dto.OTP)
                throw new BadRequestException(Messages.InvalidOtp);

            user.EmailConfirmed = true;
            user.UpdatedAt = DateTime.UtcNow;
            user.Status = UserStatus.Active;

            _memoryCache.Remove(cacheKey);

            var blockKey = $"OTP_RESEND_BLOCK_{dto.Email}";
            _memoryCache.Remove(blockKey);

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new BadRequestException(Messages.SomethingWentWrong);

            await AfterConfirmEmailAsync(user);

            return new OperationResultModel
            {
                IsSuccess = true,
                Message = Messages.EmailConfirmedSuccessfully
            };
        }

        public async Task<AuthModel> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user is null)
                throw new BadRequestException(Messages.AccountNotFound);

            CheckUser(user);

            if (!user.EmailConfirmed)
            {
                await SendOtpAsync(dto.Email);
                throw new ForbiddenException(Messages.EmailNotConfirmed);
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);

            if (!result.Succeeded)
                throw new BadRequestException(Messages.WrongPassword);

            var accessTokenExpiration = DateTime.UtcNow.AddMinutes(30);
            var accessToken = await CreateAccessTokenAsync(user, accessTokenExpiration);
            var refreshToken = CreateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = dto.RememberMe ? DateTime.UtcNow.AddDays(30) : DateTime.UtcNow.AddDays(1);
            user.Status = UserStatus.Active;
            user.LastLoginAt = DateTime.UtcNow;
            var updatedUser = await CheckDailyLoginReward(user);

            await _userManager.UpdateAsync(updatedUser);

            var auditLog = new AuditLog
            {
                UserId = user.Id,
                Action = AuditAction.Login,
                ActorType = AuditActorType.User,
                EntityName = nameof(ApplicationUser),
                EntityId = user.Id,
                Description = "User login",
                IpAddress = _requestContext.IpAddress,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<AuditLog>().AddAsync(auditLog);
            await _unitOfWork.CompleteAsync();

            return new AuthModel
            {
                AccessToken = accessToken,
                AccessTokenExpiration = accessTokenExpiration,
                RefreshToken = refreshToken
            };
        }

        public async Task<AuthModel> RefreshTokenAsync(RefreshTokenDto dto)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.RefreshToken == dto.RefreshToken);

            if (user is null)
                throw new UnauthorizedException(Messages.InvalidRefreshToken);

            CheckUser(user);

            if (user.RefreshTokenExpiryTime < DateTime.UtcNow)
                throw new UnauthorizedException(Messages.RefreshTokenExpired);

            var accessTokenExpiration = DateTime.UtcNow.AddMinutes(30);
            var accessToken = await CreateAccessTokenAsync(user, accessTokenExpiration);
            var newRefreshToken = CreateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.LastLoginAt = DateTime.UtcNow;

            var updatedUser = await CheckDailyLoginReward(user);
            await _userManager.UpdateAsync(updatedUser);

            var auditLog = new AuditLog
            {
                UserId = user.Id,
                Action = AuditAction.Login,
                ActorType = AuditActorType.User,
                EntityName = nameof(ApplicationUser),
                EntityId = user.Id,
                Description = "User login with refresh token",
                IpAddress = _requestContext.IpAddress,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<AuditLog>().AddAsync(auditLog);
            await _unitOfWork.CompleteAsync();

            return new AuthModel
            {
                AccessToken = accessToken,
                AccessTokenExpiration = accessTokenExpiration,
                RefreshToken = newRefreshToken
            };
        }

        public async Task<OperationResultModel> ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user is null)
                throw new BadRequestException(Messages.AccountNotFound);

            CheckUser(user);

            await SendForgotPasswordOtpAsync(dto.Email);

            return new OperationResultModel
            {
                IsSuccess = true,
                Message = Messages.ForgotPasswordEmailSent
            };
        }

        public async Task<OperationResultModel> VerifyForgotPasswordAsync(VerifyForgotPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user is null)
                throw new BadRequestException(Messages.AccountNotFound);

            var cacheKey = $"FORGOT_PASSWORD_OTP_{dto.Email}";

            if (!_memoryCache.TryGetValue(cacheKey, out string? storedOtp))
                throw new BadRequestException(Messages.OtpExpired);

            if (storedOtp != dto.Otp)
                throw new BadRequestException(Messages.InvalidOtp);

            _memoryCache.Remove(cacheKey);

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            return new OperationResultModel
            {
                IsSuccess = true,
                Message = resetToken
            };
        }

        public async Task<OperationResultModel> ResetPasswordAsync(ResetPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user is null)
                throw new BadRequestException(Messages.AccountNotFound);

            var result = await _userManager.ResetPasswordAsync(user,dto.ResetToken,dto.NewPassword);

            if (!result.Succeeded)
            {
                var error = result.Errors.First();

                throw new Exception(error.Description);
            }

            user.UpdatedAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
            return new OperationResultModel
            {
                IsSuccess = true,
                Message = Messages.PasswordResetSuccessfully
            };
        }

        public async Task<OperationResultModel> ChangePasswordAsync(string email, ChangePasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user is null)
                throw new BadRequestException(Messages.AccountNotFound);

            CheckUser(user);

            var result = await _userManager.ChangePasswordAsync(user,dto.CurrentPassword,dto.NewPassword);

            if (!result.Succeeded)
            {
                var error = result.Errors.First();

                throw new Exception(error.Description);
            }

            user.UpdatedAt = DateTime.UtcNow;

            await _userManager.UpdateAsync(user);

            return new OperationResultModel
            {
                IsSuccess = true,
                Message = Messages.PasswordChangedSuccessfully
            };
        }

        public async Task<OperationResultModel> DeleteAccountAsync(string email, DeleteAccountDto dto)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user is null)
                throw new BadRequestException(Messages.AccountNotFound);

            CheckUser(user);

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.CurrentPassword, false);

            if (!result.Succeeded)
                throw new BadRequestException(Messages.WrongPassword);

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
                Message = Messages.AccountDeletedSuccessfully
            };
        }

        public async Task<AuthModel> GoogleResponseAsync()
        {
            var result = await _httpContextAccessor.HttpContext!.AuthenticateAsync("External");

            if (!result.Succeeded || result.Principal is null)
                throw new UnauthorizedException(Messages.SomethingWentWrong);

            var email = result.Principal.FindFirstValue(ClaimTypes.Email);
            var name = result.Principal.FindFirstValue(ClaimTypes.Name);

            if (string.IsNullOrWhiteSpace(email))
                throw new UnauthorizedException(Messages.SomethingWentWrong);

            var user = await _userManager.FindByEmailAsync(email);

            if (user is null)
            {
                user = new ApplicationUser
                {
                    Email = email,
                    UserName = email,
                    FullName = name ?? email,
                    EmailConfirmed = true,
                    Status = UserStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createResult = await _userManager.CreateAsync(user);
                await AfterConfirmEmailAsync(user);
                if (!createResult.Succeeded)
                    throw new BadRequestException(Messages.SomethingWentWrong);

                var roleResult = await _userManager.AddToRoleAsync(user, "Customer");

                if (!roleResult.Succeeded)
                {
                    await _userManager.DeleteAsync(user);
                    throw new BadRequestException(Messages.SomethingWentWrong);
                }
            }
            else
            {
                CheckUser(user);
            }

            var googleId = result.Principal.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(googleId))
                throw new UnauthorizedException(Messages.SomethingWentWrong);

            var logins = await _userManager.GetLoginsAsync(user);

            var googleLoginExists = logins.Any(x =>
                x.LoginProvider == GoogleDefaults.AuthenticationScheme &&
                x.ProviderKey == googleId);

            if (!googleLoginExists)
            {
                var loginInfo = new UserLoginInfo(GoogleDefaults.AuthenticationScheme, googleId, "Google");

                var addLoginResult = await _userManager.AddLoginAsync(user, loginInfo);

                if (!addLoginResult.Succeeded)
                    throw new BadRequestException(Messages.SomethingWentWrong);
            }

            var accessTokenExpiration = DateTime.UtcNow.AddMinutes(30);
            var accessToken = await CreateAccessTokenAsync(user, accessTokenExpiration);

            var refreshToken = CreateRefreshToken();

            user.EmailConfirmed = true;
            user.Status = UserStatus.Active;
            user.LastLoginAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(30);

            var updateResult = await _userManager.UpdateAsync(user);
            await AfterConfirmEmailAsync(user);
            if (!updateResult.Succeeded)
                throw new BadRequestException(Messages.SomethingWentWrong);

            return new AuthModel
            {
                AccessToken = accessToken,
                AccessTokenExpiration = accessTokenExpiration,
                RefreshToken = refreshToken
            };
        }

        public async Task<OperationResultModel> LogoutAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user is null)
                throw new UnauthorizedException(Messages.AccountNotFound);

            CheckUser(user);

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                throw new BadRequestException(Messages.SomethingWentWrong);

            var auditLog = new AuditLog
            {
                UserId = user.Id,
                Action = AuditAction.Logout,
                ActorType = AuditActorType.User,
                EntityName = nameof(ApplicationUser),
                EntityId = user.Id,
                Description = "User logout",
                IpAddress = _requestContext.IpAddress,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<AuditLog>().AddAsync(auditLog);
            await _unitOfWork.CompleteAsync();

            return new OperationResultModel
            {
                IsSuccess = true,
                Message = Messages.LogoutSuccessfully
            };
        }


        #region Helping functions
        private async Task SendOtpAsync(string email)
        {
            var cacheKey = $"OTP_{email}";
            var lastSentKey = $"OTP_LAST_SENT_{email}";
            var resendCountKey = $"OTP_RESEND_COUNT_{email}";

            if (_memoryCache.TryGetValue(cacheKey, out _))
                return;

            var otpCode = GenerateOtp();

            _memoryCache.Set(lastSentKey,DateTime.UtcNow,TimeSpan.FromMinutes(30));

            _memoryCache.Set(resendCountKey,1,TimeSpan.FromMinutes(30));

            _memoryCache.Set(cacheKey, otpCode, TimeSpan.FromMinutes(5));

            await SendVerifyEmailAsync(email, otpCode);

        }
        private async Task SendVerifyEmailAsync(string email, string otpCode)
        {
            var emailBody = VerifyEmailTemplate.Generate(otpCode);

            await _emailService.SendEmailAsync(
                email,
                "Verify Your Email - MK Galácticos",
                emailBody);
        }
        private string GenerateOtp()
        {
            return RandomNumberGenerator
                .GetInt32(0, 1_000_000)
                .ToString("D6");
        }
        private void CheckUser(ApplicationUser user)
        {

            if (user.Status == UserStatus.Deleted)
                throw new BadRequestException(Messages.AccountDeleted);

            if (user.Status == UserStatus.Banned)
                throw new BadRequestException(Messages.AccountBanned);
        }
        private async Task<string> CreateAccessTokenAsync(ApplicationUser user, DateTime accessTokenExpiration)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.Email, user.Email!)
            };

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecurityKey"]!));

            var token = new JwtSecurityToken(
                audience: _configuration["JwtSettings:ValidAudience"],
                issuer: _configuration["JwtSettings:ValidIssuer"],
                claims: claims,
                expires: accessTokenExpiration,
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha512));

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        private string CreateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var generator = RandomNumberGenerator.Create();
            generator.GetBytes(randomNumber);

            return Convert.ToBase64String(randomNumber);
        }
        private async Task SendForgotPasswordOtpAsync(string email)
        {
            var cacheKey = $"FORGOT_PASSWORD_OTP_{email}";

            if (_memoryCache.TryGetValue(cacheKey, out _))
                return;

            var otpCode = GenerateOtp();

            _memoryCache.Set(cacheKey, otpCode, TimeSpan.FromMinutes(5));

            await ForgotPasswordEmailAsync(email, otpCode);
        }
        private async Task ForgotPasswordEmailAsync(string email, string otpCode)
        {
            var emailBody = ForgotPasswordTemplate.Generate(otpCode);

            await _emailService.SendEmailAsync(
                email,
                "Reset Your Password - MK Galácticos",
                emailBody);
        }
        
        private static readonly JsonSerializerOptions AuditJsonOptions = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Converters =
                {
                    new JsonStringEnumConverter()
                }
        };

        private async Task<ApplicationUser> CheckDailyLoginReward(ApplicationUser user)
        {
            var LoyaltyPointSpec = new BaseSpecification<LoyaltyPointTransaction>(x => x.UserId == user.Id && x.Reason == LoyaltyTransactionReason.DailyLogin && x.CreatedAt.Date == DateTime.UtcNow.Date);

            if (!await _unitOfWork.Repository<LoyaltyPointTransaction>().AnyAsync(LoyaltyPointSpec))
            {
                await _unitOfWork.Repository<LoyaltyPointTransaction>().AddAsync(new LoyaltyPointTransaction
                {
                    UserId = user.Id,
                    Points = 5,
                    Type = LoyaltyPointTransactionType.Earn,
                    Reason = LoyaltyTransactionReason.DailyLogin,
                    CreatedAt = DateTime.UtcNow
                });

                await _unitOfWork.CompleteAsync();

                user.LifetimeEarnedLoyaltyPoints += 5;
                user.LoyaltyPointsBalance += 5;
            }
            return user;
        }

        private async Task AfterConfirmEmailAsync(ApplicationUser user)
        {
            var wishlistSpec = new BaseSpecification<Wishlist>(x => x.UserId == user.Id);

            if (!await _unitOfWork.Repository<Wishlist>().AnyAsync(wishlistSpec))
            {
                await _unitOfWork.Repository<Wishlist>().AddAsync(new Wishlist
                {
                    UserId = user.Id
                });

                await _unitOfWork.CompleteAsync();
            }

            var LoyaltyPointSpec = new BaseSpecification<LoyaltyPointTransaction>(x => x.UserId == user.Id && x.Reason == LoyaltyTransactionReason.Referral);

            if (!await _unitOfWork.Repository<LoyaltyPointTransaction>().AnyAsync(LoyaltyPointSpec))
            {
                await _unitOfWork.Repository<LoyaltyPointTransaction>().AddAsync(new LoyaltyPointTransaction
                {
                    UserId = user.Id,
                    Points = 200,
                    Type = LoyaltyPointTransactionType.Earn,
                    Reason = LoyaltyTransactionReason.Referral,
                    CreatedAt = DateTime.UtcNow
                }); 
                user.LifetimeEarnedLoyaltyPoints += 200;
                user.LoyaltyPointsBalance += 200;
                await _unitOfWork.CompleteAsync();
            }
            await _couponService.AssignWelcomeCouponAsync(user.Id);
        }
        #endregion
    }
}
