using MK.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.DTOs.PaymentMethods
{
    public class PaymentMethodDto
    {
        public int Id { get; set; }
        public string IconImageUrl { get; set; } = null!;
        public string Name { get; set; } = null!;
        public PaymentMethodStatus Status { get; set; }
    }
}
