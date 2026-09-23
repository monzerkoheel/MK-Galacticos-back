using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.DTOs.Products
{
    public class ManagmentProductRatingDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public string UserFullName { get; set; } = null!;
        public string? UserImageUrl { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
