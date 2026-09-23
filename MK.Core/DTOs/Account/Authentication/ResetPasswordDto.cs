using MK.Core.Localization.Resources;
using System.ComponentModel.DataAnnotations;

namespace MK.Core.DTOs.Account.Authentication
{
    public class ResetPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
        [Required]
        public string ResetToken { get; set; } = null!;
        [Required]
        [RegularExpression(@"^(?=.{8,20}$)(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]+$",
            ErrorMessageResourceType = typeof(Messages),
            ErrorMessageResourceName = nameof(Messages.PasswordRequirements)
        )]
        public string NewPassword { get; set; } = null!;
    }
}
