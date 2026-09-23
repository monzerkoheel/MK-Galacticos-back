using Microsoft.AspNetCore.Http;
using MK.Core.Models;


namespace MK.Core.DTOs.PaymentMethods
{
    public class UpdatePaymentMethodDto
    {
        public IFormFile? IconImage { get; set; }
        public PaymentMethodStatus? Status { get; set; }
        public ICollection<AddPaymentMethodTranslationDto> Translations { get; set; } = new List<AddPaymentMethodTranslationDto>();
    }
}
