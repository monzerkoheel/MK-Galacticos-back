using System.ComponentModel.DataAnnotations;


namespace MK.Core.DTOs.Account.Authentication
{
    public class ChangeEmailDto
    {
        [Required]
        [EmailAddress]
        public string CurrentEmail { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string NewEmail { get; set; } = null!;
    }
}
