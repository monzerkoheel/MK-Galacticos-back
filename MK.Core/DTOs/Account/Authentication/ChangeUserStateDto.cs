using System.ComponentModel.DataAnnotations;

namespace MK.Core.DTOs.Account.Authentication
{
    public enum UserNewState
    {
        Active,
        Inactive
    }
    public class ChangeUserStateDto
    {
        [Required]
        public string UserId { get; set; } = null!;

        [Required]
        public UserNewState UserNewState { get; set; }
    }
}
