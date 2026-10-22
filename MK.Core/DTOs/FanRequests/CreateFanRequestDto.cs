using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace MK.Core.DTOs.FanRequests
{
    public class CreateFanRequestDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [Required]
        [MaxLength(1000)]
        public string Description { get; set; } = null!;

        [Required]
        public IFormFile Image { get; set; } = null!;
    }
}
