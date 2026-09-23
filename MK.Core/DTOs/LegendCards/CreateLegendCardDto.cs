using Microsoft.AspNetCore.Http;
using MK.Core.HelperModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.DTOs.LegendCards
{
    public class CreateLegendCardDto
    {
        [Required]
        public string Name { get; set; } = null!;
        [Required]
        public IFormFile Image { get; set; } = null!;
        [Required]
        public decimal Price { get; set; }
        public Visibility Status { get; set; } = Visibility.Inactive;
    }
}
