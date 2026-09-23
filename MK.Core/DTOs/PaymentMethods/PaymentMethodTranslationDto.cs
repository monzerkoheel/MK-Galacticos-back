using MK.Core.HelperModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.DTOs.PaymentMethods
{
    public class PaymentMethodTranslationDto
    {
        public Language Language { get; set; }
        public string Name { get; set; } = null!;
    }
}
