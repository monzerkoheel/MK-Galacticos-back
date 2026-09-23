using MK.Core.HelperModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.DTOs.PaymentMethods
{
    public class AddPaymentMethodTranslationDto
    {
        [Required]
        public Language Language { get; set; }
        [Required]
        public string Name { get; set; } = null!;
    }
}
