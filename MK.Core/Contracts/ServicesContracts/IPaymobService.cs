using MK.Core.HelperModels.Paymob.RequestsAndResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.Contracts.ServicesContracts
{
    public interface IPaymobService
    {
        Task<PaymentIntentResult> CreatePaymentIntentAsync(int orderId);

        PaymobWebhookRequest VerifyAndParseWebhook(string rawBody, string? hmac);

        Task<PaymentStatusResult> GetTransactionStatusAsync(long transactionId);
        Task HandlePaymobWebhookAsync(string rawBody, string? hmac);

        Task<RefundResult> RefundAsync(RefundRequest request);
    }
}
