using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.HelperModels.Paymob.RequestsAndResults
{
    public class RefundResult
    {
        public long TransactionId { get; set; }
        public bool Success { get; set; }
        public bool IsRefunded { get; set; }
        public long RefundedAmountCents { get; set; }
    }
}
