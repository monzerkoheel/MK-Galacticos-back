using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.HelperModels.Paymob.RequestsAndResults
{
    public class PaymentIntentResult
    {
        public long IntentionOrderId { get; set; }
        public string Id { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string PublicKey { get; set; } = string.Empty;
    }
}
