using MK.Core.DTOs.Account.Authentication;
using MK.Core.HelperModels;
using MK.Core.Models;

namespace MK.Core.Contracts.ServicesContracts
{
    public interface IAuthService
    {
        Task<OperationResultModel> RegisterAsync(RegisterDto dto);
        Task<OperationResultModel> ResendOtpAsync(EmailDto dto);
        Task<OperationResultModel> ChangeEmailAsync(ChangeEmailDto dto);
        Task<OperationResultModel> ConfirmEmailAsync(ConfirmEmailDto dto);
        Task<AuthModel> LoginAsync(LoginDto dto);
        Task<AuthModel> RefreshTokenAsync(RefreshTokenDto dto);
        Task<OperationResultModel> ForgotPasswordAsync(ForgotPasswordDto dto);
        Task<OperationResultModel> VerifyForgotPasswordAsync(VerifyForgotPasswordDto dto);
        Task<OperationResultModel> ResetPasswordAsync(ResetPasswordDto dto);
        Task<OperationResultModel> ChangePasswordAsync(string email, ChangePasswordDto dto);
        Task<OperationResultModel> LogoutAsync(string email);
        Task<OperationResultModel> DeleteAccountAsync(string email, DeleteAccountDto dto);
        Task<AuthModel> GoogleResponseAsync();
    }
}
