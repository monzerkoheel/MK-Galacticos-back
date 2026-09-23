using MK.Core.Localization.Resources;
using System.ComponentModel.DataAnnotations;

namespace MK.Core.DTOs.Account.Authentication
{
    public class RegisterDto
    {
        [Required]
        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = "";

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [RegularExpression(@"^(?=.{8,20}$)(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]+$",
            ErrorMessageResourceType = typeof(Messages),
            ErrorMessageResourceName = nameof(Messages.PasswordRequirements)
        )]
        public string Password { get; set; } = null!;

    }
}
