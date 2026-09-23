using Microsoft.AspNetCore.Http;
using MK.Core.HelperModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.DTOs.LegendCards
{
    public class UpdateLegendCardDto
    {
        public string? Name { get; set; }
        public IFormFile? Image { get; set; }
        public decimal? Price { get; set; }
        public Visibility? Status { get; set; }
    }
}
