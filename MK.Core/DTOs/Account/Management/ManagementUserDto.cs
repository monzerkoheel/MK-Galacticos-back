using MK.Core.Models;

namespace MK.Core.DTOs.Account.Management
{
    public class ManagementUserDto
    {
        public string Id { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string Role { get; set; } = null!;
        public UserStatus Status { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
