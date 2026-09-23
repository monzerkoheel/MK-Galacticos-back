using System.Text.Json.Serialization;

namespace MK.Core.HelperModels.Paymob.RequestsAndResults
{
    public class PaymobOrder
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("amount_cents")]
        public long AmountCents { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = string.Empty;

        [JsonPropertyName("merchant_order_id")]
        public string? MerchantOrderId { get; set; }
    }
}
