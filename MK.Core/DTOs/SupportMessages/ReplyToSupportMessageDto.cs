using System.ComponentModel.DataAnnotations;

namespace MK.Core.DTOs.SupportMessages
{
    public class ReplyToSupportMessageDto
    {
        [Required]
        [MaxLength(5000)]
        public string Reply { get; set; } = null!;
    }
}
