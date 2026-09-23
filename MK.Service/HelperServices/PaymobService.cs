using Microsoft.Extensions.Options;
using MK.Core.Contracts.RepositoriesContracts;
using MK.Core.Contracts.ServicesContracts;
using MK.Core.HelperModels;
using MK.Core.HelperModels.Exceptions;
using MK.Core.HelperModels.Paymob.RequestsAndResults;
using MK.Core.HelperModels.Paymob_Configuration;
using MK.Core.Localization;
using MK.Core.Models;
using MK.Repository.Specifications.OrderSpecifications;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MK.Service.HelperServices
{
    public class PaymobService : IPaymobService
    {
        private readonly PaymobOptions _options;
        private readonly HttpClient _httpClient;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentLanguage _currentLanguage;

        public PaymobService(IOptions<PaymobOptions> options,HttpClient httpClient,IUnitOfWork unitOfWork, ICurrentLanguage currentLanguage)
        {
            _options = options.Value;
            _httpClient = httpClient;
            _unitOfWork = unitOfWork;
            _currentLanguage = currentLanguage;
        }
        public async Task<PaymentIntentResult> CreatePaymentIntentAsync(int orderId)
        {
            var orderSpec = new OrderWithAllDetailsSpecification(x => x.Id == orderId);

            var order = await _unitOfWork.Repository<Order>().GetFirstOrDefaultAsync(orderSpec);

            if (order is null)
                throw new NotFoundException("Order not found.");

            if (order.Status != OrderStatus.Pending)
                throw new BadRequestException("Order cannot be paid.");

            if (order.Payment is null)
                throw new BadRequestException("Order payment was not found.");

            var amount = (long)Math.Round(order.TotalAmount * 100,MidpointRounding.AwayFromZero);

            var numbers = Regex
                .Matches(order.Shipping.DetailedAddress, @"\d+")
                .Select(x => x.Value)
                .ToList();

            var apartment = numbers.Count >= 1 ? numbers[^1] : "NA";
            var building = numbers.Count >= 2 ? numbers[^2] : "NA";
            var floor = numbers.Count >= 3 ? numbers[^3] : "NA";

            var fullNameParts = order.User.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var firstName = fullNameParts.FirstOrDefault() ?? "NA";

            var lastName = fullNameParts.Length > 1? string.Join(" ", fullNameParts.Skip(1)): "NA";

            var paymobItems = order.Items.Select(item => new PaymobItem
            {
                Name = item.Translations.FirstOrDefault(x => x.Language == _currentLanguage.Language)?.ProductName ?? "Product",
                Amount = (long)Math.Round(item.UnitPrice * 100, MidpointRounding.AwayFromZero),
                Description = item.Translations.FirstOrDefault(x => x.Language == _currentLanguage.Language)?.ProductName ?? "Product",
                Quantity = item.Quantity
            }).ToList();
            if (order.ShippingCost > 0)
                paymobItems.Add(new PaymobItem { Name = "Shipping", Amount = (long)(order.ShippingCost * 100), Quantity = 1});

            if (order.LegendCardPrice is > 0)
                paymobItems.Add(new PaymobItem { Name = "Legend Card", Amount = (long)(order.LegendCardPrice.Value * 100), Quantity = 1});

            if (order.CouponDiscountAmount is > 0)
                paymobItems.Add(new PaymobItem { Name = "Discount", Amount = -(long)(order.CouponDiscountAmount.Value * 100), Quantity = 1});
            var paymobRequest = new PaymobPaymentIntentRequest
            {
                Amount = paymobItems.Sum(x => x.Amount * x.Quantity),

                Currency = _options.Currency,

                PaymentMethods =
                [
                    _options.IntegrationId
                ],

                SpecialReference = order.Id.ToString(),

                Expiration = 3600,

                Items = paymobItems,

                BillingData = new PaymobBillingData
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Email = order.User.Email,
                    PhoneNumber = order.Shipping.PhoneNumber,
                    Apartment = apartment,
                    Building = building,
                    Floor = floor,
                    Street = order.Shipping.Area,
                    City = order.Shipping.Governorate,
                    State = order.Shipping.Governorate,
                    Country = order.Shipping.Country
                },
                NotificationUrl = "https://localhost:7228/api/order/paymob-webhook"
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, "v1/intention/")
            {
                Content = JsonContent.Create(paymobRequest)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Token", _options.SecretKey);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new BadRequestException($"Paymob payment intent failed: {error}");
            }

            var paymobResponse = await response.Content.ReadFromJsonAsync<PaymobPaymentIntentResponse>();

            if (paymobResponse is null || string.IsNullOrWhiteSpace(paymobResponse.ClientSecret))
            {
                throw new BadRequestException("Invalid response received from Paymob.");
            }

            // Update the Payment that already belongs to the Order.
            order.Payment.Status = PaymentStatus.Pending;
            order.Payment.TransactionId = paymobResponse.IntentionOrderId.ToString();
            order.Payment.PaymentDetails = JsonSerializer.Serialize(new
            {
                IntentionOrderId = paymobResponse.IntentionOrderId,
                IntentionId = paymobResponse.Id
            });
            _unitOfWork.Repository<Order>().Update(order);
            await _unitOfWork.CompleteAsync();

            return new PaymentIntentResult
            {
                IntentionOrderId = paymobResponse.IntentionOrderId,
                Id = paymobResponse.Id,
                ClientSecret = paymobResponse.ClientSecret,
                PublicKey = _options.PublicKey
            };
        }
        public Task<PaymentStatusResult> GetTransactionStatusAsync(long transactionId)
        {
            throw new NotImplementedException();
        }
        public PaymobWebhookRequest VerifyAndParseWebhook(string rawBody, string? hmac)
        {
            if (string.IsNullOrWhiteSpace(hmac))
                throw new ForbiddenException("Missing HMAC signature.");

            using var document = JsonDocument.Parse(rawBody);
            var root = document.RootElement;

            if (!root.TryGetProperty("obj", out var obj))
                throw new BadRequestException("Invalid Paymob webhook payload.");

            var sourceData = obj.TryGetProperty("source_data", out var sd) ? sd : default;
            var order = obj.TryGetProperty("order", out var ord) ? ord : default;

            static string Extract(JsonElement parent, string propertyName)
            {
                if (parent.ValueKind == JsonValueKind.Undefined ||
                    !parent.TryGetProperty(propertyName, out var value))
                    return string.Empty;

                return value.ValueKind switch
                {
                    JsonValueKind.String => value.GetString() ?? string.Empty,
                    JsonValueKind.True => "true",
                    JsonValueKind.False => "false",
                    JsonValueKind.Number => value.GetRawText(),
                    _ => string.Empty
                };
            }

            var concatenated = string.Concat(
                Extract(obj, "amount_cents"),
                Extract(obj, "created_at"),
                Extract(obj, "currency"),
                Extract(obj, "error_occured"),
                Extract(obj, "has_parent_transaction"),
                Extract(obj, "id"),
                Extract(obj, "integration_id"),
                Extract(obj, "is_3d_secure"),
                Extract(obj, "is_auth"),
                Extract(obj, "is_capture"),
                Extract(obj, "is_refunded"),
                Extract(obj, "is_standalone_payment"),
                Extract(obj, "is_voided"),
                Extract(order, "id"),
                Extract(obj, "owner"),
                Extract(obj, "pending"),
                Extract(sourceData, "pan"),
                Extract(sourceData, "sub_type"),
                Extract(sourceData, "type"),
                Extract(obj, "success"));

            var computedHash = HMACSHA512.HashData(
                Encoding.UTF8.GetBytes(_options.HmacSecret),
                Encoding.UTF8.GetBytes(concatenated));

            var computedHex = Convert.ToHexString(computedHash).ToLowerInvariant();

            var isValid = CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(computedHex),
                Encoding.UTF8.GetBytes(hmac.Trim().ToLowerInvariant()));

            if (!isValid)
                throw new ForbiddenException("Invalid Paymob HMAC signature.");

            var webhookRequest = JsonSerializer.Deserialize<PaymobWebhookRequest>(rawBody);
            if (webhookRequest?.Obj is null)
                throw new BadRequestException("Invalid Paymob webhook payload.");

            return webhookRequest;
        }
        public async Task HandlePaymobWebhookAsync(string rawBody, string? hmac)
        {
            var webhook = VerifyAndParseWebhook(rawBody, hmac);
            var obj = webhook.Obj;

            var orderIdAtPaymob = obj.Order.Id.ToString();
            var orderSpec = new OrderWithAllDetailsSpecification(x => x.Payment.TransactionId == orderIdAtPaymob);
            var order = await _unitOfWork.Repository<Order>().GetFirstOrDefaultAsync(orderSpec);

            if (order is null)
                throw new NotFoundException($"No order found matching Paymob order id {orderIdAtPaymob}.");

            if (obj.IsRefunded)
                await HandleRefundedPaymentAsync(order, obj);
            else if (obj.IsVoided)
                await HandleCancelledPaymentAsync(order, obj);
            else if (obj.Success && !obj.Pending)
                await HandleSuccessfulPaymentAsync(order, obj);
            else if (!obj.Success && !obj.Pending)
                await HandleFailedPaymentAsync(order, obj);
            // لو obj.Pending == true (زي بعض محافظ الموبايل)، مفيش حاجة تتغير دلوقتي، هيجيلك callback تاني لما تتحسم.
        }

        #region Paymob Webhook Handlers (TODO: هتضيف منطق تغيير الـ State هنا)

        private Task HandleSuccessfulPaymentAsync(Order order, PaymobTransactionObject obj)
        {
            // TODO: Order.Status = ... / Payment.Status = Paid / Payment.PaidAt = ...
            // TODO: مهم — سجل obj.Id (مش order.id) في مكان تقدر تسترجعه بعدين، ده transaction id الحقيقي اللي هتحتاجه لو عملت Refund
            return Task.CompletedTask;
        }

        private Task HandleFailedPaymentAsync(Order order, PaymobTransactionObject obj)
        {
            // TODO: Order.Status = ... / Payment.Status = Failed
            // TODO: استرجاع الـ Stock + الكوبون + نقاط الولاء (نفس reversal logic اللي اتفقنا عليه قبل كده)
            return Task.CompletedTask;
        }

        private Task HandleCancelledPaymentAsync(Order order, PaymobTransactionObject obj)
        {
            // TODO: Order.Status = Cancelled + استرجاع الـ Stock/الكوبون/النقاط
            return Task.CompletedTask;
        }

        private Task HandleRefundedPaymentAsync(Order order, PaymobTransactionObject obj)
        {
            // TODO: Order.Status = ReFunded + استرجاع الـ Stock/الكوبون/النقاط
            return Task.CompletedTask;
        }

        #endregion
        public async Task<RefundResult> RefundAsync(RefundRequest request)
        {
            var refundPayload = new
            {
                transaction_id = request.TransactionId,
                amount_cents = request.AmountCents
            };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/acceptance/void_refund/refund")
            {
                Content = JsonContent.Create(refundPayload)
            };
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Token", _options.SecretKey);

            var response = await _httpClient.SendAsync(httpRequest);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new BadRequestException($"Paymob refund failed: {error}");
            }

            var refundResponse = await response.Content.ReadFromJsonAsync<PaymobWebhookRequest>();
            if (refundResponse?.Obj is null)
                throw new BadRequestException("Invalid response received from Paymob refund API.");

            return new RefundResult
            {
                TransactionId = refundResponse.Obj.Id,
                Success = refundResponse.Obj.Success,
                IsRefunded = refundResponse.Obj.IsRefunded,
                RefundedAmountCents = refundResponse.Obj.RefundedAmountCents
            };
        }
    }
}
