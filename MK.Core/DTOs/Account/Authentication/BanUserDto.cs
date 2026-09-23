using System.ComponentModel.DataAnnotations;

namespace MK.Core.DTOs.Account.Authentication
{
    public class BanUserDto
    {
        public DateTime? BannedUntil { get; set; }

        [Required]
        [MaxLength(500)]
        public string? BanReason { get; set; }
    }
}
