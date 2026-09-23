using System.ComponentModel.DataAnnotations;

namespace MK.Core.DTOs.Account.Authentication
{
    public class ForgotPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
    }
}
