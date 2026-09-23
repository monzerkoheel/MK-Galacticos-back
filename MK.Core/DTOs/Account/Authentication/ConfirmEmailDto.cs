using System.ComponentModel.DataAnnotations;


namespace MK.Core.DTOs.Account.Authentication
{
    public class ConfirmEmailDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string OTP { get; set; } = null!;
    }
}
