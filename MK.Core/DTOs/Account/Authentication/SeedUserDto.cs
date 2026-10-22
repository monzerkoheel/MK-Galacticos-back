using MK.Core.Models;

namespace MK.Core.DTOs.Account.Authentication
{
    public class SeedUserDto
    {
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string Password { get; set; } = null!;
        public bool EmailConfirmed { get; set; }
        public UserStatus Status { get; set; }
        public string Role { get; set; } = null!;
    }
}
