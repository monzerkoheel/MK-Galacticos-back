using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.DTOs.Orders
{
    public enum PaymentMethodType
    {
        Cash,
        Paymob,
    }
    public class CheckoutResponseDto
    {
        public bool IsSuccess { get; set; }
        public int OrderId { get; set; }
        public string OrderNumber { get; set; }
        public PaymentMethodType PaymentMethodType { get; set; }
        public string? ClientSecret { get; set; }
        public string? PublicKey { get; set; }
    }
}
