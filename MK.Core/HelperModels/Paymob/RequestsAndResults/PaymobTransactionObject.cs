using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MK.Core.HelperModels.Paymob.RequestsAndResults
{
    public class PaymobTransactionObject
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("pending")]
        public bool Pending { get; set; }

        [JsonPropertyName("amount_cents")]
        public long AmountCents { get; set; }

        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("is_auth")]
        public bool IsAuth { get; set; }

        [JsonPropertyName("is_capture")]
        public bool IsCapture { get; set; }

        [JsonPropertyName("is_standalone_payment")]
        public bool IsStandalonePayment { get; set; }

        [JsonPropertyName("is_voided")]
        public bool IsVoided { get; set; }

        [JsonPropertyName("is_refunded")]
        public bool IsRefunded { get; set; }

        [JsonPropertyName("is_3d_secure")]
        public bool Is3DSecure { get; set; }

        [JsonPropertyName("integration_id")]
        public int IntegrationId { get; set; }

        [JsonPropertyName("order")]
        public PaymobOrder Order { get; set; } = new();

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = string.Empty;

        [JsonPropertyName("error_occured")]
        public bool ErrorOccured { get; set; }

        [JsonPropertyName("refunded_amount_cents")]
        public long RefundedAmountCents { get; set; }

        [JsonPropertyName("is_captured")]
        public bool IsCaptured { get; set; }

        [JsonPropertyName("captured_amount")]
        public long CapturedAmount { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [JsonPropertyName("has_parent_transaction")]
        public bool HasParentTransaction { get; set; }

        [JsonPropertyName("owner")]
        public long Owner { get; set; }

        [JsonPropertyName("source_data")]
        public PaymobSourceData? SourceData { get; set; }
    }
    public class PaymobSourceData
    {
        [JsonPropertyName("pan")]
        public string Pan { get; set; } = string.Empty;

        [JsonPropertyName("sub_type")]
        public string SubType { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
    }
}
