using System.ComponentModel.DataAnnotations;

namespace MK.Core.DTOs.Account.Authentication
{
    public class VerifyForgotPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
        [Required]
        public string Otp { get; set; } = null!;
    }
}
