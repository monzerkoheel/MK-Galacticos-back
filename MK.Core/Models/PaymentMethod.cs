using MK.Core.HelperModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.Models
{
    public enum PaymentMethodStatus
    {
        Available,
        CommingSoon,
        Inactive
    }
    public class PaymentMethod : ModelBase
    {
        public string? IconImageUrl { get; set; }
        public PaymentMethodStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public ICollection<PaymentMethodTranslation> Translations { get; set; } = new List<PaymentMethodTranslation>();
    }
}
