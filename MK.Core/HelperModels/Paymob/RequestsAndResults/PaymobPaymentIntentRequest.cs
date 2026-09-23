using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MK.Core.HelperModels.Paymob.RequestsAndResults
{
    public class PaymobPaymentIntentRequest
    {
        [JsonPropertyName("amount")]
        public long Amount { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = string.Empty;

        [JsonPropertyName("payment_methods")]
        public List<int> PaymentMethods { get; set; } = [];

        [JsonPropertyName("items")]
        public List<PaymobItem> Items { get; set; } = [];

        [JsonPropertyName("billing_data")]
        public PaymobBillingData BillingData { get; set; } = new();

        [JsonPropertyName("special_reference")]
        public string SpecialReference { get; set; } = string.Empty;

        [JsonPropertyName("expiration")]
        public int Expiration { get; set; } = 3600;

        [JsonPropertyName("notification_url")]
        public string? NotificationUrl { get; set; }
    }
}
