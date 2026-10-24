using System.ComponentModel.DataAnnotations;

namespace MK.Core.DTOs.SupportMessages
{
    public class CreateSupportMessageDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; set; } = null!;

        [Required]
        [MaxLength(2000)]
        public string Message { get; set; } = null!;
    }
}
