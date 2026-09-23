using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.DTOs.PaymentMethods
{
    public class PaymentMethodDetailsDto
    {
        public int Id { get; set; }
        public string? IconImageUrl { get; set; }
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public ICollection<PaymentMethodTranslationDto> Translations { get; set; } = new List<PaymentMethodTranslationDto>();
    }
}
