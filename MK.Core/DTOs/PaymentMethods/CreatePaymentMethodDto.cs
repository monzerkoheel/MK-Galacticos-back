using Microsoft.AspNetCore.Http;
using MK.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.DTOs.PaymentMethods
{
    public class CreatePaymentMethodDto
    {
        [Required]
        public IFormFile IconImage { get; set; }
        [Required]
        public PaymentMethodStatus Status { get; set; }
        public ICollection<AddPaymentMethodTranslationDto> Translations { get; set; } = new List<AddPaymentMethodTranslationDto>();
    }
}
