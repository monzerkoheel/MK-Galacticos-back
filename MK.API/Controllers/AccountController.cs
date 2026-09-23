using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MK.API.Errors;
using MK.Core.Contracts.ServicesContracts;
using MK.Core.DTOs.Account.Authentication;
using MK.Core.HelperModels;
using MK.Core.HelperModels.Exceptions;
using MK.Core.Localization.Resources;
using MK.Core.Models;
using MK.Service.Services;
using System.Security.Claims;

namespace MK.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;
        public AccountController(IAuthService authService, IConfiguration configuration)
        {
            _authService = authService;
            _configuration = configuration;
        }

        [HttpPost("register")] // /api/Account/register
        public async Task<ActionResult<OperationResultModel>> Register(RegisterDto model) 
            => (await _authService.RegisterAsync(model));

        [HttpPost("resend-otp")]
        public async Task<ActionResult<OperationResultModel>> ResendOtp(EmailDto dto)
            => Ok(await _authService.ResendOtpAsync(dto));
       
        [HttpPut("edit-email")]
        public async Task<ActionResult<OperationResultModel>> EditEmail(ChangeEmailDto model) 
            => Ok(await _authService.ChangeEmailAsync(model));
       
        [HttpPost("confirm-email")]
        public async Task<ActionResult<OperationResultModel>> ConfirmEmail(ConfirmEmailDto model) 
            => Ok(await _authService.ConfirmEmailAsync(model));

        [HttpPost("login")]
        public async Task<ActionResult<AuthModel>> Login(LoginDto model)
            => Ok(await _authService.LoginAsync(model));

        [HttpPost("refresh-token")]
        public async Task<ActionResult<AuthModel>> RefreshToken(RefreshTokenDto model)
            => Ok(await _authService.RefreshTokenAsync(model));

        [HttpPost("forgot-password")]
        public async Task<ActionResult<OperationResultModel>> ForgotPassword(ForgotPasswordDto dto)
            => Ok(await _authService.ForgotPasswordAsync(dto));

        [HttpPost("verify-forgot-password")]
        public async Task<ActionResult<OperationResultModel>> VerifyForgotPassword(VerifyForgotPasswordDto dto)
            => Ok(await _authService.VerifyForgotPasswordAsync(dto));

        [HttpPost("reset-password")]
        public async Task<ActionResult<OperationResultModel>> ResetPassword(ResetPasswordDto dto)
            => Ok(await _authService.ResetPasswordAsync(dto));

        [Authorize]
        [HttpPut("change-password")]
        public async Task<ActionResult<OperationResultModel>> ChangePassword(ChangePasswordDto dto)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);

            if (email is null)
                return Unauthorized(new ApiResponse(401));

            var result = await _authService.ChangePasswordAsync(email,dto);

            return Ok(result);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<ActionResult<OperationResultModel>> Logout()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);

            if (email is null)
                return Unauthorized(new ApiResponse(401));

            return Ok(await _authService.LogoutAsync(email));
        }

        [Authorize]
        [HttpDelete("delete-account")]
        public async Task<ActionResult<OperationResultModel>> DeleteAccount(DeleteAccountDto dto)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);

            if (email is null)
                return Unauthorized(new ApiResponse(401));

            var result = await _authService.DeleteAccountAsync(email, dto);

            return Ok(result);
        }

        [HttpGet("google-login")]
        public IActionResult GoogleLogin()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(GoogleResponse))
            };

            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("google-response")]
        public async Task<ActionResult<AuthModel>> GoogleResponse()
        {
            var result = await _authService.GoogleResponseAsync();
            var frontendBase = _configuration["FrontendUrl"];

            var frontendUrl = $"{frontendBase}/auth/google-callback.html"
                + $"?accessToken={Uri.EscapeDataString(result.AccessToken)}"
                + $"&refreshToken={Uri.EscapeDataString(result.RefreshToken)}"
                + $"&expiration={Uri.EscapeDataString(result.AccessTokenExpiration.ToString("o"))}";

            return Redirect(frontendUrl);
        }

    }
}
