using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.HelperModels.Paymob.RequestsAndResults
{
    public class RefundRequest
    {
        public long TransactionId { get; set; }
        public long AmountCents { get; set; }
    }
}
