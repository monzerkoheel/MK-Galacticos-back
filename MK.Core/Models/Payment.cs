using MK.Core.HelperModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.Models
{
    public enum PaymentStatus
    {
        Pending,
        Paid,
        Failed,
        Cancelled,
        Refunded
    }
    public class Payment : ModelBase
    {
        public int OrderId { get; set; }

        public int PaymentMethodId { get; set; }

        public PaymentStatus Status { get; set; }

        public decimal Amount { get; set; }

        public string? TransactionId { get; set; }

        public string? PaymentDetails { get; set; }

        public DateTime? PaidAt { get; set; }

        public DateTime CreatedAt { get; set; }


        public Order Order { get; set; } = null!;

        public PaymentMethod PaymentMethod { get; set; } = null!;
    }
}
