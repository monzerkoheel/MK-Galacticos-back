using System.ComponentModel.DataAnnotations;

namespace MK.Core.DTOs.Account.Authentication
{
    public class RefreshTokenDto
    {
        [Required]
        public string RefreshToken { get; set; } = null!;
    }
}
