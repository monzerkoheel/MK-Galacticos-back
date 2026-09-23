using AutoMapper;
using Azure;
using Microsoft.AspNetCore.Identity;
using MK.Core.Contracts.RepositoriesContracts;
using MK.Core.Contracts.ServicesContracts;
using MK.Core.DTOs.Orders;
using MK.Core.HelperModels;
using MK.Core.HelperModels.Exceptions;
using MK.Core.Localization;
using MK.Core.Localization.Resources;
using MK.Core.Models;
using MK.Repository.Specifications;
using MK.Repository.Specifications.OrderSpecifications;


namespace MK.Service.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly ICurrentLanguage _currentLanguage;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPaymobService _paymentGetwayService;
        private readonly IInvoiceService _invoiceService;
        private readonly IEmailService _emailService;
        public OrderService(IUnitOfWork unitOfWork, 
                            ICurrentUserService currentUserService, 
                            ICurrentLanguage currentLanguage, 
                            IMapper mapper, 
                            UserManager<ApplicationUser> userManager,
                            IPaymobService paymentGetwayService,
                            IEmailService emailService,
                            IInvoiceService invoiceService) 
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUserService;
            _currentLanguage = currentLanguage;
            _mapper = mapper;
            _paymentGetwayService = paymentGetwayService;
            _userManager = userManager;
            _emailService = emailService;
            _invoiceService = invoiceService;
        }

        public async Task<CheckoutResponseDto> OrderCheckOutAsync(CreateOrderDto dto)
        {
            if (!dto.HasLegendCard && dto.LegendCardId is not null)
                throw new ArgumentException("LegendCardId should be null when HasLegendCard is false.");

            if (!dto.HasCoupon && dto.CouponCode is not null)
                throw new ArgumentException("UserCouponId should be null when HasCoupon is false.");

            if (dto.HasLegendCard && dto.LegendCardId is null)
                throw new ArgumentException("LegendCardId should not be null when HasLegendCard is true.");

            if (dto.HasCoupon && dto.CouponCode is null)
                throw new ArgumentException("UserCouponId should not be null when HasCoupon is true.");

            var cartSpec = new BaseSpecification<Cart>(x => x.UserId == _currentUser.UserId);
            cartSpec.AddInclude(x => x.Items);
            var cart = await _unitOfWork.Repository<Cart>().GetFirstOrDefaultAsync(cartSpec);
            if (cart is null || cart.Items.Count <1)
                throw new BadRequestException("You Can not create an order with an empty cart.");

            var shppingMethodSpec = new BaseSpecification<ShippingMethod>(x => x.Id == dto.ShippingMethodId && x.Status == Visibility.Active);
            shppingMethodSpec.AddInclude(x => x.Translations);
            var shippingMethod = await _unitOfWork.Repository<ShippingMethod>().GetFirstOrDefaultAsync(shppingMethodSpec);
            if (shippingMethod is null)
                throw new BadRequestException("Invalid shipping method.");

            var paymentMethodSpec = new BaseSpecification<PaymentMethod>(x => x.Id == dto.PaymentMethodId && x.Status == PaymentMethodStatus.Available);
            paymentMethodSpec.AddInclude(x => x.Translations);
            var paymentMethod = await _unitOfWork.Repository<PaymentMethod>().GetFirstOrDefaultAsync(paymentMethodSpec);
            if (paymentMethod is null)
                throw new BadRequestException("Invalid payment method.");

            UserCoupon? coupon = null;
            if (dto.CouponCode is not null)
            {
                var userCouponSpec = new BaseSpecification<UserCoupon>(x => x.UserId == _currentUser.UserId && x.Code == dto.CouponCode);
                userCouponSpec.AddInclude(x => x.Coupon);
                coupon = await _unitOfWork.Repository<UserCoupon>().GetFirstOrDefaultAsync(userCouponSpec);
                if (coupon is null)
                    throw new BadRequestException("Invalid coupon code.");
                if (coupon.IsUsed)
                    throw new BadRequestException("Coupon code has already been used.");
                if (coupon.ExpirationDate < DateTime.UtcNow)
                    throw new BadRequestException("Coupon code is expired.");
            }

            LegendCard? legendCard = null;
            if (dto.LegendCardId is not null)
            {
                var legendCardSpec = new BaseSpecification<LegendCard>(x => x.Id == dto.LegendCardId && x.Status == Visibility.Active);
                legendCard = await _unitOfWork.Repository<LegendCard>().GetFirstOrDefaultAsync(legendCardSpec);
                if (legendCard is null)
                    throw new BadRequestException("Invalid legend card.");
            }

            var productsIds = cart.Items.Select(x => x.ProductId).ToList();
            var productsSpec = new ProductWithAllDataSpecification(x => productsIds.Contains(x.Id));
            var products = await _unitOfWork.Repository<Product>().ListAsync(productsSpec);
            decimal supTotal = 0;
            decimal discountAmount = 0;
            List<OrderItem> orderItems = new List<OrderItem>();
            foreach (var item in cart.Items)
            {
                var product = products.FirstOrDefault(x => x.Id == item.ProductId);
                var stock = product?.Stocks.FirstOrDefault(x => x.Size == item.Size && x.StockType == item.Gender);
                if (stock!.StockQuantity < item.Quantity)
                    throw new BadRequestException($"Product {product!.Translations.FirstOrDefault(x => x.Language == _currentLanguage.Language)!.Name} does not have enough stock.");
                if (product!.SalePrice is null || product!.SalePrice < 1)
                    supTotal += item.Quantity * product.Price;
                if (product!.SalePrice is not null && product!.SalePrice > 0)
                {
                    discountAmount += item.Quantity * (product.Price - product.SalePrice.Value);
                    supTotal += item.Quantity * product.SalePrice.Value;
                }
                string productImageUrl = product!.Images.FirstOrDefault(x => x.IsMain && x.Gender == ProductGender.Men)!.ImageUrl; ;
                if (item .Gender == ProductGender.Women)
                    productImageUrl = product!.Images.FirstOrDefault(x => x.IsMain && x.Gender == ProductGender.Women)!.ImageUrl;
                var price = product.SalePrice ?? product.Price;
                string? printName = item.CustomPrintName;
                int? printNumber = item.CustomPrintNumber;
                if (!item.IsCustomPrint && item.ProductPrintId is not null)
                {
                    var productPrint = product.Prints.FirstOrDefault(x => x.Id == item.ProductPrintId);
                    if (productPrint is null)
                        throw new BadRequestException("التصميم المختار لمنتج {productName} لم يعد متاحًا، برجاء إعادة اختيار الطباعة.");
                    printName = productPrint.Name;
                    printNumber = productPrint.Number;
                }
                
                var orderItem = new OrderItem
                {
                    ProductId = product!.Id,
                    ProductImageUrl = productImageUrl,
                    Gender = item.Gender,
                    Size = item.Size,
                    Quantity = item.Quantity,
                    UnitPrice = price,
                    TotalPrice = price * item.Quantity,
                    IsCustomPrint = item.IsCustomPrint,
                    ProductPrintId = item.ProductPrintId,
                    PrintName = printName,
                    PrintNumber = printNumber,
                    Translations = _mapper.Map<List<OrderItemTranslation>>(product.Translations)
                };
                orderItems.Add(orderItem);
            }
            decimal total = supTotal;
            total += shippingMethod.Price;
            decimal shippingCost = shippingMethod.Price;
            if (legendCard is not null)
                total += legendCard.Price;
            decimal couponDiscount = 0;
            if (coupon is not null)
            {
                var couponCoupon = coupon.Coupon;
                if (couponCoupon.MinimumOrderAmount is not null && total < couponCoupon.MinimumOrderAmount)
                    throw new BadRequestException("Order amount does not meet the minimum requirement for this coupon.");
                if (couponCoupon.CouponType == CouponType.FreeShippingUpgrade)
                {
                    var upgraderShippingSpec = new BaseSpecification<ShippingMethod>(x => x.Price > shippingMethod.Price && x.Status == Visibility.Active);
                    upgraderShippingSpec.AddOrderBy(x => x.Price);
                    var upgraderShippingMethods = await _unitOfWork.Repository<ShippingMethod>().GetFirstOrDefaultAsync(upgraderShippingSpec);
                    if (upgraderShippingMethods is null)
                        throw new BadRequestException("No shipping method is available for upgrading from your current method.");
                    shippingMethod = upgraderShippingMethods;
                }
                else if (couponCoupon.CouponType == CouponType.FreeShipping)
                {
                    total -= shippingMethod.Price;
                    couponDiscount += shippingMethod.Price;
                }
                else if (couponCoupon.CouponType == CouponType.FixedAmount && couponCoupon.DiscountAmount is not null)
                {
                    if (couponCoupon.DiscountAmount >= total)
                        throw new BadRequestException("Coupon discount amount exceeds order total.");
                    total -= couponCoupon.DiscountAmount ?? 0;
                    couponDiscount += couponCoupon.DiscountAmount ?? 0;
                }
                else if (couponCoupon.CouponType == CouponType.Percentage && couponCoupon.DiscountPercentage is not null)
                {
                    if (couponCoupon.MaximumDiscountAmount is not null)
                    {
                        var discount = total * couponCoupon.DiscountPercentage.Value / 100;
                        if (discount > couponCoupon.MaximumDiscountAmount)
                            discount = couponCoupon.MaximumDiscountAmount.Value;
                        total -= discount;
                        couponDiscount += discount;
                    }
                    else
                    {
                        var discount = total * (couponCoupon.DiscountPercentage.Value / 100);
                        total -= discount;
                        couponDiscount += discount;
                    }

                }
                else
                {
                    throw new BadRequestException("Sorry we can not handel your coupon right now u can countiue checkout with it and contact with support and we well solf this problem.");
                }              
                    
            }

            var orderShipping = new OrderShipping
            {
                ShippingMethodId = shippingMethod.Id,
                ShippingCost = shippingMethod.Price,
                EstimatedDeliveryDate = DateTime.UtcNow.AddDays(shippingMethod.EstimatedDeliveryDays),
                CreatedAt = DateTime.UtcNow,
                RecipientName = dto.RecipientName,
                Country = dto.Country,
                Governorate = dto.Governorate,
                Area = dto.Area,
                DetailedAddress = dto.DetailedAddress,
                PhoneNumber = dto.PhoneNumber,
                Notes = dto.Notes
            };

            Payment orderPayment = new Payment
            {
                PaymentMethodId = paymentMethod.Id,
                Status = PaymentStatus.Pending,
                Amount = total,
                CreatedAt = DateTime.UtcNow
            };
            bool isCashPayment = (paymentMethod.Translations.FirstOrDefault(x => x.Language == Language.English)?.Name.ToLower().Contains("cash") == true);

            await _unitOfWork.BeginTransactionAsync();
            string? clientSecret = null;
            string? publicKey = null;
            var orderStatus = OrderStatus.Processing;
            var orderNumber = await GenerateOrderNumberAsync();
            Order? order = null;
            int? orderId = null;
            try
            {
                if (isCashPayment)
                    orderPayment.PaymentDetails = "Cash On Delivery";
                else if (!isCashPayment)
                {
                    orderPayment.PaymentDetails = "Online Payment";
                    orderStatus = OrderStatus.Pending;
                }

                order = new Order
                {
                    OrderNumber = orderNumber,
                    UserId = _currentUser.UserId,
                    Status = orderStatus,
                    Subtotal = supTotal,
                    ShippingCost = shippingCost,
                    DiscountAmount = discountAmount,
                    TotalAmount = total,
                    HasCoupon = dto.HasCoupon,
                    UserCouponId = coupon?.Id,
                    CouponCode = coupon?.Code,
                    CouponDiscountAmount = couponDiscount,
                    HasLegendCard = dto.HasLegendCard,
                    LegendCardId = legendCard?.Id,
                    LegendCardPrice = legendCard?.Price,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Shipping = orderShipping,
                    Items = orderItems,
                    Payment = orderPayment
                };

                await _unitOfWork.Repository<Order>().AddAsync(order);
                await _unitOfWork.CompleteAsync();
                var orderSpec = new BaseSpecification<Order>(x => x.OrderNumber == orderNumber);
                order = await _unitOfWork.Repository<Order>().GetFirstOrDefaultAsync(orderSpec);
                orderId = order?.Id;

                _unitOfWork.Repository<Cart>().Delete(cart);
                if (coupon is not null)
                {
                    coupon.IsUsed = true;
                    coupon.UsedAt = DateTime.UtcNow;
                    _unitOfWork.Repository<UserCoupon>().Update(coupon);
                }
                foreach (var item in orderItems)
                {
                    var product = products.FirstOrDefault(x => x.Id == item.ProductId);
                    var stock = product?.Stocks.FirstOrDefault(x => x.Size == item.Size && x.StockType == item.Gender);
                    if (stock is not null)
                        stock.StockQuantity -= item.Quantity;
                    var totalStockQuantity = product.Stocks.Sum(x => x.StockQuantity);
                    if (totalStockQuantity < 1)
                        product.IsOutOfStock = true;
                    else
                        product.IsOutOfStock = false;
                }
                _unitOfWork.Repository<Product>().UpdateRange(products);
                int newPoints = (int)Math.Ceiling(total / 10);
                var user = await _userManager.FindByIdAsync(_currentUser.UserId!);
                user!.LifetimeEarnedLoyaltyPoints += newPoints;
                user!.LoyaltyPointsBalance += newPoints;
                var loyaltyPointsTransaction = new LoyaltyPointTransaction
                {
                    UserId = user.Id,
                    Points = newPoints,
                    Type = LoyaltyPointTransactionType.Earn,
                    Reason = LoyaltyTransactionReason.Purchase,
                    OrderId = orderId,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.Repository<LoyaltyPointTransaction>().AddAsync(loyaltyPointsTransaction);
                var result = await _unitOfWork.CompleteAsync();
                if (result < 1)
                    throw new Exception(Messages.SomethingWentWrong);
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }

            if (!isCashPayment)
            {
                try
                {
                    var getWayResult = await _paymentGetwayService.CreatePaymentIntentAsync(orderId!.Value);
                    clientSecret = getWayResult.ClientSecret;
                    publicKey = getWayResult.PublicKey;

                }
                catch (Exception error)
                {

                    throw new Exception($"Paymob payment intent failed: {error}");
                }
            }                

            return new CheckoutResponseDto
            {
                IsSuccess = true,
                OrderId = orderId ?? 0,
                OrderNumber = orderNumber,
                PaymentMethodType = isCashPayment ? PaymentMethodType.Cash : PaymentMethodType.Paymob,
                ClientSecret = clientSecret,
                PublicKey = publicKey
            };
        }


        public async Task<OperationResultModel> SendOrderInvoiceToUserAsync(int orderId)
        {
            var orderSpec = new OrderWithAllDetailsSpecification(x => x.Id == orderId);
            var order = await _unitOfWork.Repository<Order>().GetFirstOrDefaultAsync(orderSpec);
            if (order is null)
                throw new NotFoundException(string.Format(Messages.NotFound,"Order"));
            if (order.UserId != _currentUser.UserId)
                throw new ForbiddenException("This order does not belong to you.");
            try
            {
                await SendOrderInvoiceAsync(order);
            }
            catch
            {
                throw new Exception(Messages.SomethingWentWrong);
            }

            return new OperationResultModel
            {
                IsSuccess = true,
                Message = "Invoice was sent to your Email"
            };
        }

        #region Helper Methods
        private async Task<string> GenerateOrderNumberAsync()
        {
            var year = DateTime.UtcNow.Year % 100;
            var lastOrderSpec = new BaseSpecification<Order>(x => x.OrderNumber.StartsWith($"MK{year}-"));
            lastOrderSpec.AddOrderByDes(x => x.OrderNumber);
            var lastOrder = await _unitOfWork.Repository<Order>().GetFirstOrDefaultAsync(lastOrderSpec);
            var nextNumber = 1;
            if (lastOrder is not null)
            {
                var lastSequence = int.Parse(lastOrder.OrderNumber.Split('-')[1]);
                nextNumber = lastSequence + 1;
            }

            return $"MK{year}-{nextNumber:D5}";
        }

        private async Task SendOrderInvoiceAsync(Order order)
        {
            var pdf = await _invoiceService.GenerateInvoiceAsync(
                order,
                _currentLanguage.Code.ToString());

            var emailBody = $"""
                <!DOCTYPE html>
                <html lang="en">
                <head>
                    <meta charset="UTF-8">
                    <meta name="viewport" content="width=device-width, initial-scale=1.0">

                    <title>MK Galácticos - Order Confirmation</title>
                </head>

                <body style="
                    margin:0;
                    padding:0;
                    background-color:#f4f4f4;
                    font-family:Arial,Helvetica,sans-serif;
                    color:#222;
                ">

                <table
                    width="100%"
                    cellpadding="0"
                    cellspacing="0"
                    border="0"
                    style="background-color:#f4f4f4;padding:30px 15px;"
                >
                    <tr>
                        <td align="center">

                            <!-- MAIN CONTAINER -->
                            <table
                                width="600"
                                cellpadding="0"
                                cellspacing="0"
                                border="0"
                                style="
                                    max-width:600px;
                                    width:100%;
                                    background-color:#ffffff;
                                    border-radius:12px;
                                    overflow:hidden;
                                "
                            >

                                <!-- HEADER -->
                                <tr>
                                    <td
                                        align="center"
                                        style="
                                            background-color:#0A0A0A;
                                            padding:30px 20px;
                                            border-bottom:3px solid #D4AF37;
                                        "
                                    >
                                        <div style="
                                            font-size:26px;
                                            font-weight:bold;
                                            letter-spacing:2px;
                                            color:#D4AF37;
                                        ">
                                            MK GALÁCTICOS
                                        </div>

                                        <div style="
                                            margin-top:8px;
                                            font-size:12px;
                                            color:#AAAAAA;
                                            letter-spacing:1px;
                                        ">
                                            FOOTBALL • STYLE • LEGENDS
                                        </div>
                                    </td>
                                </tr>


                                <!-- CONTENT -->
                                <tr>
                                    <td style="padding:40px 35px;">

                                        <!-- TITLE -->
                                        <div style="
                                            font-size:24px;
                                            font-weight:bold;
                                            color:#111111;
                                            margin-bottom:10px;
                                        ">
                                            Thank you for your order! ⚽
                                        </div>

                                        <div style="
                                            font-size:15px;
                                            line-height:1.7;
                                            color:#666666;
                                            margin-bottom:30px;
                                        ">
                                            Your order has been successfully confirmed.
                                            We're getting your items ready.
                                        </div>


                                        <!-- ORDER CARD -->
                                        <table
                                            width="100%"
                                            cellpadding="0"
                                            cellspacing="0"
                                            border="0"
                                            style="
                                                background-color:#151515;
                                                border-radius:10px;
                                                overflow:hidden;
                                            "
                                        >
                                            <tr>
                                                <td style="padding:22px;">

                                                    <div style="
                                                        font-size:11px;
                                                        color:#D4AF37;
                                                        text-transform:uppercase;
                                                        letter-spacing:1px;
                                                        margin-bottom:6px;
                                                    ">
                                                        Order Number
                                                    </div>

                                                    <div style="
                                                        font-size:20px;
                                                        font-weight:bold;
                                                        color:#ffffff;
                                                        margin-bottom:20px;
                                                    ">
                                                        #{order.OrderNumber}
                                                    </div>


                                                    <table
                                                        width="100%"
                                                        cellpadding="0"
                                                        cellspacing="0"
                                                        border="0"
                                                    >
                                                        <tr>

                                                            <td width="50%">
                                                                <div style="
                                                                    font-size:11px;
                                                                    color:#999999;
                                                                    margin-bottom:5px;
                                                                ">
                                                                    TOTAL
                                                                </div>

                                                                <div style="
                                                                    font-size:18px;
                                                                    font-weight:bold;
                                                                    color:#D4AF37;
                                                                ">
                                                                    {order.TotalAmount:N2} EGP
                                                                </div>
                                                            </td>


                                                            <td
                                                                width="50%"
                                                                align="right"
                                                            >
                                                                <div style="
                                                                    font-size:11px;
                                                                    color:#999999;
                                                                    margin-bottom:5px;
                                                                ">
                                                                    STATUS
                                                                </div>

                                                                <div style="
                                                                    font-size:14px;
                                                                    color:#ffffff;
                                                                    font-weight:bold;
                                                                ">
                                                                    Confirmed
                                                                </div>
                                                            </td>

                                                        </tr>
                                                    </table>

                                                </td>
                                            </tr>
                                        </table>


                                        <!-- INVOICE MESSAGE -->
                                        <div style="
                                            margin-top:30px;
                                            padding:20px;
                                            background-color:#fafafa;
                                            border-left:4px solid #D4AF37;
                                            border-radius:6px;
                                        ">

                                            <div style="
                                                font-size:15px;
                                                font-weight:bold;
                                                color:#222222;
                                                margin-bottom:7px;
                                            ">
                                                Your invoice is attached
                                            </div>

                                            <div style="
                                                font-size:13px;
                                                line-height:1.7;
                                                color:#666666;
                                            ">
                                                We've attached your order invoice as a PDF
                                                file for your records.
                                            </div>

                                        </div>


                                        <!-- SHIPPING MESSAGE -->
                                        <div style="
                                            margin-top:25px;
                                            font-size:13px;
                                            line-height:1.7;
                                            color:#666666;
                                        ">
                                            We'll keep you updated as your order moves
                                            through the shipping process.
                                        </div>


                                        <!-- DIVIDER -->
                                        <div style="
                                            height:1px;
                                            background-color:#eeeeee;
                                            margin:30px 0;
                                        "></div>


                                        <!-- THANK YOU -->
                                        <div align="center">

                                            <div style="
                                                font-size:16px;
                                                font-weight:bold;
                                                color:#222222;
                                                margin-bottom:8px;
                                            ">
                                                Thank you for choosing
                                                <span style="color:#D4AF37;">
                                                    MK Galácticos
                                                </span>
                                            </div>

                                            <div style="
                                                font-size:13px;
                                                color:#999999;
                                                line-height:1.6;
                                            ">
                                                We appreciate your support and love for
                                                the beautiful game.
                                            </div>

                                        </div>

                                    </td>
                                </tr>


                                <!-- FOOTER -->
                                <tr>
                                    <td
                                        align="center"
                                        style="
                                            background-color:#0A0A0A;
                                            padding:25px 20px;
                                            border-top:1px solid #222222;
                                        "
                                    >

                                        <div style="
                                            font-size:14px;
                                            font-weight:bold;
                                            color:#D4AF37;
                                            letter-spacing:1px;
                                        ">
                                            MK GALÁCTICOS
                                        </div>

                                        <div style="
                                            margin-top:8px;
                                            font-size:11px;
                                            color:#777777;
                                        ">
                                            Football shirts made for the fans.
                                        </div>

                                        <div style="
                                            margin-top:12px;
                                            font-size:10px;
                                            color:#555555;
                                        ">
                                            © 2026 MK Galácticos. All rights reserved.
                                        </div>

                                    </td>
                                </tr>

                            </table>

                        </td>
                    </tr>
                </table>

                </body>
                </html>
                """;

            await _emailService.SendEmailAsync(
                order.User.Email,
                $"MK Galácticos - Invoice #{order.OrderNumber}",
                emailBody,
                pdf,
                $"Invoice-{order.OrderNumber}.pdf");
        }

        #endregion
    }
}
