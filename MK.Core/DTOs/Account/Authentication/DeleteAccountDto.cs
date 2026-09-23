using System.ComponentModel.DataAnnotations;

namespace MK.Core.DTOs.Account.Authentication
{
    public class DeleteAccountDto
    {
        [Required]
        public string CurrentPassword { get; set; } = null!;
    }
}
