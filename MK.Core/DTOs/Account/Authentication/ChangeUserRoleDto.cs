using System.ComponentModel.DataAnnotations;

namespace MK.Core.DTOs.Account.Authentication
{
    public class ChangeUserRoleDto
    {
        [Required]
        public string UserId { get; set; } = null!;

        [Required]
        public string NewRole { get; set; } = null!;
    }
}
