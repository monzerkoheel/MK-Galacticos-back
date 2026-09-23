using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace MK.Core.DTOs.Account.Information
{
    public class UploadUserImageDto
    {
        [Required]
        public IFormFile UserImage {  get; set; }
    }
}
