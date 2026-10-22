using AutoMapper;
using Azure;
using Microsoft.AspNetCore.Identity;
using MK.Core.Contracts.RepositoriesContracts;
using MK.Core.Contracts.ServicesContracts;
using MK.Core.DTOs;
using MK.Core.DTOs.Account.Authentication;
using MK.Core.DTOs.Orders;
using MK.Core.HelperModels;
using MK.Core.HelperModels.Exceptions;
using MK.Core.Localization;
using MK.Core.Localization.Resources;
using MK.Core.Models;
using MK.Core.Specifications.SpecificationParameters;
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
        private readonly IInvoiceService _invoiceService;
        private readonly IEmailService _emailService;
        private readonly IUserManagementService _userManagementService;
        private readonly IFileService _fileService;
        public OrderService(IUnitOfWork unitOfWork, 
                            ICurrentUserService currentUserService, 
                            ICurrentLanguage currentLanguage, 
                            IMapper mapper, 
                            UserManager<ApplicationUser> userManager,
                            IEmailService emailService,
                            IInvoiceService invoiceService,
                            IUserManagementService userManagementService,
                            IFileService fileService) 
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUserService;
            _currentLanguage = currentLanguage;
            _mapper = mapper;
            _userManager = userManager;
            _emailService = emailService;
            _invoiceService = invoiceService;
            _userManagementService = userManagementService;
            _fileService = fileService;
        }

        public async Task<CheckoutResponseDto> OrderCheckOutAsync(CreateOrderDto dto)
        {
            if (!dto.HasLegendCard && dto.LegendCardId is not null)
                throw new ArgumentException(Messages.LegendCardIdShouldBeNull);

            if (!dto.HasCoupon && dto.CouponCode is not null)
                throw new ArgumentException(Messages.CouponCodeShouldBeNull);

            if (dto.HasLegendCard && dto.LegendCardId is null)
                throw new ArgumentException(Messages.LegendCardIdRequired);

            if (dto.HasCoupon && dto.CouponCode is null)
                throw new ArgumentException(Messages.CouponCodeRequired);

            var cartSpec = new BaseSpecification<Cart>(x => x.UserId == _currentUser.UserId);
            cartSpec.AddInclude(x => x.Items);
            var cart = await _unitOfWork.Repository<Cart>().GetFirstOrDefaultAsync(cartSpec);
            if (cart is null || cart.Items.Count <1)
                throw new BadRequestException(Messages.EmptyCart);

            var shppingMethodSpec = new BaseSpecification<ShippingMethod>(x => x.Id == dto.ShippingMethodId && x.Status == Visibility.Active);
            shppingMethodSpec.AddInclude(x => x.Translations);
            var shippingMethod = await _unitOfWork.Repository<ShippingMethod>().GetFirstOrDefaultAsync(shppingMethodSpec);
            if (shippingMethod is null)
                throw new BadRequestException(Messages.InvalidShippingMethod);

            var paymentMethodSpec = new BaseSpecification<PaymentMethod>(x => x.Id == dto.PaymentMethodId && x.Status == PaymentMethodStatus.Available);
            paymentMethodSpec.AddInclude(x => x.Translations);
            var paymentMethod = await _unitOfWork.Repository<PaymentMethod>().GetFirstOrDefaultAsync(paymentMethodSpec);
            if (paymentMethod is null)
                throw new BadRequestException(Messages.InvalidPaymentMethod);

            UserCoupon? coupon = null;
            if (dto.CouponCode is not null)
            {
                var userCouponSpec = new BaseSpecification<UserCoupon>(x => x.UserId == _currentUser.UserId && x.Code == dto.CouponCode);
                userCouponSpec.AddInclude(x => x.Coupon);
                coupon = await _unitOfWork.Repository<UserCoupon>().GetFirstOrDefaultAsync(userCouponSpec);
                if (coupon is null)
                    throw new BadRequestException(Messages.InvalidCouponCode);
                if (coupon.IsUsed)
                    throw new BadRequestException(Messages.CouponAlreadyUsed);
                if (coupon.ExpirationDate < DateTime.UtcNow)
                    throw new BadRequestException(Messages.CouponExpired);
            }

            LegendCard? legendCard = null;
            if (dto.LegendCardId is not null)
            {
                var legendCardSpec = new BaseSpecification<LegendCard>(x => x.Id == dto.LegendCardId && x.Status == Visibility.Active);
                legendCard = await _unitOfWork.Repository<LegendCard>().GetFirstOrDefaultAsync(legendCardSpec);
                if (legendCard is null)
                    throw new BadRequestException(Messages.InvalidLegendCard);
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
                    throw new BadRequestException(string.Format(Messages.InsufficientProductStock, product!.Translations.FirstOrDefault(x => x.Language == _currentLanguage.Language)!.Name));
                
                supTotal += item.Quantity * product.Price;  
                if (product.SalePrice is not null && product.SalePrice > 0)
                    discountAmount += item.Quantity * (product.Price - product.SalePrice.Value);

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
                        throw new BadRequestException(Messages.ProductPrintUnavailable);
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
            decimal total = supTotal - discountAmount;   
            total += shippingMethod.Price;
            decimal shippingCost = shippingMethod.Price;
            if (legendCard is not null)
                total += legendCard.Price;
            decimal couponDiscount = 0;
            if (coupon is not null)
            {
                var couponCoupon = coupon.Coupon;
                if (couponCoupon.MinimumOrderAmount is not null && total < couponCoupon.MinimumOrderAmount)
                    throw new BadRequestException(Messages.CouponMinimumOrderAmountNotMet);
                if (couponCoupon.CouponType == CouponType.FreeShippingUpgrade)
                {
                    var upgraderShippingSpec = new BaseSpecification<ShippingMethod>(x => x.Price > shippingMethod.Price && x.Status == Visibility.Active);
                    upgraderShippingSpec.AddOrderBy(x => x.Price);
                    var upgraderShippingMethods = await _unitOfWork.Repository<ShippingMethod>().GetFirstOrDefaultAsync(upgraderShippingSpec);
                    if (upgraderShippingMethods is null)
                        throw new BadRequestException(Messages.NoShippingMethodForUpgrade);
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
                        throw new BadRequestException(Messages.CouponDiscountExceedsOrderTotal);
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
                    throw new BadRequestException(Messages.UnsupportedCouponType);
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

            /*if (!isCashPayment)
            {
                try
                {
                    var getWayResult = ONLINE PaymentGetWay Integration
                    clientSecret = getWayResult.ClientSecret;
                    publicKey = getWayResult.PublicKey;

                }
                catch (Exception error)
                {

                    throw new Exception($"Paymob payment intent failed: {error}");
                }
            }*/                

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

        public async Task<OperationResultModel> CancelOrderAsync(int orderId)
        {
            var orderSpec = new OrderWithAllDetailsSpecification(x => x.Id == orderId);
            var order = await _unitOfWork.Repository<Order>().GetFirstOrDefaultAsync(orderSpec);

            if (order is null)
                throw new NotFoundException(string.Format(Messages.NotFound, "Order"));

            if (order.UserId != _currentUser.UserId)
                throw new BadRequestException(Messages.OrderNotBelongToUser);

            if ((DateTime.UtcNow - order.CreatedAt).TotalHours >= 8)
                throw new BadRequestException(Messages.OrderCancellationTimeExpired);

            if (order.Status is not (OrderStatus.Pending or OrderStatus.Processing))
                throw new BadRequestException(Messages.OrderCannotBeCancelled);

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await RestoreOrderStockAsync(order);

                order.Status = OrderStatus.Cancelled;
                order.UpdatedAt = DateTime.UtcNow;
                if (order.HasCoupon)
                {
                    var couponSpec = new BaseSpecification<UserCoupon>(x => x.UserId == order.UserId && x.Code == order.CouponCode);
                    var coupon = await _unitOfWork.Repository<UserCoupon>().GetFirstOrDefaultAsync(couponSpec);
                    if (coupon is not null)
                    {
                        coupon.IsUsed = false;
                        coupon.UsedAt = null;
                        _unitOfWork.Repository<UserCoupon>().Update(coupon);
                    }
                }
                _unitOfWork.Repository<Order>().Update(order);

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

            return new OperationResultModel 
            { 
                IsSuccess = true, 
                Message = Messages.OrderCancelledSuccessfully 
            };
        }

        public async Task<OperationResultModel> ChangeOrderStateAsync(int orderId, ChangeOrderStateDto dto)
        {
            var orderSpec = new OrderWithAllDetailsSpecification(x => x.Id == orderId);
            var order = await _unitOfWork.Repository<Order>().GetFirstOrDefaultAsync(orderSpec);

            if (order is null)
                throw new NotFoundException(string.Format(Messages.NotFound, "Order"));

            if (order.Status == dto.NewStatus)
                throw new BadRequestException("Order is already in this state.");

            if (order.Status is OrderStatus.Cancelled or OrderStatus.ReFunded)
                throw new BadRequestException("Cannot change the state of a cancelled or refunded order.");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                if (dto.NewStatus is OrderStatus.Delivered)
                {
                    order.Payment.Status = PaymentStatus.Paid;
                    var orderLoyaltyPointsSpec = new BaseSpecification<LoyaltyPointTransaction>(x => x.OrderId == orderId && x.Type == LoyaltyPointTransactionType.Earn);
                    var existingLoyaltyPointsTransaction = await _unitOfWork.Repository<LoyaltyPointTransaction>().GetFirstOrDefaultAsync(orderLoyaltyPointsSpec);
                    if (existingLoyaltyPointsTransaction is null)
                    {
                        int newPoints = (int)Math.Ceiling(order.TotalAmount / 10);
                        var user = order.User;
                        user.LifetimeEarnedLoyaltyPoints += newPoints;
                        user.LoyaltyPointsBalance += newPoints;
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
                    }

                    order.Shipping.DeliveredAt = DateTime.UtcNow;
                }
                if (dto.NewStatus is OrderStatus.Cancelled)
                {
                    await RestoreOrderStockAsync(order);
                    order.Payment.Status = PaymentStatus.Cancelled;
                    var orderLoyaltyPointsSpec = new BaseSpecification<LoyaltyPointTransaction>(x => x.OrderId == orderId && x.Type == LoyaltyPointTransactionType.Earn);
                    var existingLoyaltyPointsTransaction = await _unitOfWork.Repository<LoyaltyPointTransaction>().GetFirstOrDefaultAsync(orderLoyaltyPointsSpec);
                    var redeemOrderLoyaltyPointsSpec = new BaseSpecification<LoyaltyPointTransaction>(x => x.OrderId == orderId && x.Type == LoyaltyPointTransactionType.Redeem);
                    var existingRedeemLoyaltyPointsTransaction = await _unitOfWork.Repository<LoyaltyPointTransaction>().GetFirstOrDefaultAsync(redeemOrderLoyaltyPointsSpec);
                    if (existingLoyaltyPointsTransaction is not null && existingRedeemLoyaltyPointsTransaction is null)
                    {
                        var user = order.User;
                        user.LifetimeEarnedLoyaltyPoints -= existingLoyaltyPointsTransaction.Points;
                        user.LoyaltyPointsBalance -= existingLoyaltyPointsTransaction.Points;
                        var loyaltyPointsTransaction = new LoyaltyPointTransaction
                        {
                            UserId = user.Id,
                            Points = existingLoyaltyPointsTransaction.Points,
                            Type = LoyaltyPointTransactionType.Redeem,
                            Reason = LoyaltyTransactionReason.OrderNotCompleted,
                            OrderId = orderId,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _unitOfWork.Repository<LoyaltyPointTransaction>().AddAsync(loyaltyPointsTransaction);
                    }
                }
                if (dto.NewStatus is OrderStatus.ReFunded)
                {
                    await RestoreOrderStockAsync(order);
                    order.Payment.Status = PaymentStatus.Refunded;
                    var orderLoyaltyPointsSpec = new BaseSpecification<LoyaltyPointTransaction>(x => x.OrderId == orderId && x.Type == LoyaltyPointTransactionType.Earn);
                    var existingLoyaltyPointsTransaction = await _unitOfWork.Repository<LoyaltyPointTransaction>().GetFirstOrDefaultAsync(orderLoyaltyPointsSpec);
                    var redeemOrderLoyaltyPointsSpec = new BaseSpecification<LoyaltyPointTransaction>(x => x.OrderId == orderId && x.Type == LoyaltyPointTransactionType.Redeem);
                    var existingRedeemLoyaltyPointsTransaction = await _unitOfWork.Repository<LoyaltyPointTransaction>().GetFirstOrDefaultAsync(redeemOrderLoyaltyPointsSpec);
                    if (existingLoyaltyPointsTransaction is not null && existingRedeemLoyaltyPointsTransaction is null)
                    {
                        var user = order.User;
                        user.LifetimeEarnedLoyaltyPoints -= existingLoyaltyPointsTransaction.Points;
                        user.LoyaltyPointsBalance -= existingLoyaltyPointsTransaction.Points;
                        var loyaltyPointsTransaction = new LoyaltyPointTransaction
                        {
                            UserId = user.Id,
                            Points = existingLoyaltyPointsTransaction.Points,
                            Type = LoyaltyPointTransactionType.Redeem,
                            Reason = LoyaltyTransactionReason.OrderNotCompleted,
                            OrderId = orderId,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _unitOfWork.Repository<LoyaltyPointTransaction>().AddAsync(loyaltyPointsTransaction);
                    }
                }
                    

                order.Status = dto.NewStatus;
                order.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Repository<Order>().Update(order);
                await _unitOfWork.CompleteAsync();

                if (dto.NewStatus == OrderStatus.NotReceived)
                {
                    order.Payment.Status = PaymentStatus.Failed;
                    var previousNotReceivedSpec = new BaseSpecification<Order>(x =>
                        x.UserId == order.UserId && x.Status == OrderStatus.NotReceived && x.Id != order.Id);
                    var previousCount = await _unitOfWork.Repository<Order>().CountAsync(previousNotReceivedSpec);

                    if (previousCount >= 2)
                    {
                        try
                        {
                            await _userManagementService.BanUserAsync(order.UserId, new BanUserDto
                            {
                                BannedUntil = DateTime.UtcNow.AddMonths(6),
                                BanReason = "تكرار عدم استلام الطلبات (Not Received) لأكثر من 3 مرات."
                            });
                        }
                        catch
                        {
                            // لو فشل البان (مثلاً اليوزر بانند بالفعل)، متوقفش تغيير حالة الأوردر عشان كده
                        }
                    }
                }

                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }

            return new OperationResultModel 
            { 
                IsSuccess = true, 
                Message = "Order state updated successfully." 
            };
        }

        public async Task<PaginationModel<OrderDto>> GetAllOrdersAsync(OrdersSpecPrams specParams)
        {
            var ordersSpec = new BaseSpecification<Order>(x =>
                (specParams.Status == null || x.Status == specParams.Status) &&
                (string.IsNullOrWhiteSpace(specParams.Search) ||
                    x.OrderNumber.Contains(specParams.Search) ||
                    x.User.FullName.Contains(specParams.Search) ||
                    x.User.Email.Contains(specParams.Search)));

            var totalCount = await _unitOfWork.Repository<Order>().CountAsync(ordersSpec);

            if (specParams.SortByPrice.HasValue && !specParams.SortByPriceDes.HasValue)
            {
                ordersSpec.AddOrderBy(x => x.TotalAmount);
            }
            else if (!specParams.SortByPrice.HasValue && specParams.SortByPriceDes.HasValue)
            {
                ordersSpec.AddOrderByDes(x => x.TotalAmount);
            }
            else
            {
                ordersSpec.AddOrderByDes(x => x.CreatedAt);
            }


            ordersSpec.ApplyPagination((specParams.PageIndex - 1) * specParams.PageSize, specParams.PageSize);
            ordersSpec.AddInclude(x => x.Items);
            ordersSpec.AddInclude(x => x.User);

            var orders = await _unitOfWork.Repository<Order>().ListAsync(ordersSpec);
            var data = _mapper.Map<List<OrderDto>>(orders);

            return new PaginationModel<OrderDto>(specParams.PageIndex, specParams.PageSize, totalCount, data);
        }
        
        public async Task<OrderDetailsDto> GetOrderDetailsAsync(int orderId)
        {
            var orderSpec = new OrderWithAllDetailsSpecification(x => x.Id == orderId);
            var order = await _unitOfWork.Repository<Order>().GetFirstOrDefaultAsync(orderSpec);

            if (order is null)
                throw new NotFoundException(string.Format(Messages.NotFound, "Order"));

            return MapToOrderDetailsDto(order);
        }

        public async Task<OrderDetailsDto> GetOrderByIdAsync(int orderId)
        {
            var orderSpec = new OrderWithAllDetailsSpecification(x => x.Id == orderId);
            var order = await _unitOfWork.Repository<Order>().GetFirstOrDefaultAsync(orderSpec);

            if (order is null)
                throw new NotFoundException(string.Format(Messages.NotFound, "Order"));

            if (order.UserId != _currentUser.UserId)
                throw new BadRequestException(Messages.OrderNotBelongToUser);

            return MapToOrderDetailsDto(order);
        }

        public async Task<PaginationModel<AuditLogDto>> GetOrderAuditLogsAsync(int orderId, PaginationRequest dto)
        {
            var order = await _unitOfWork.Repository<Order>().GetByIdAsync(orderId);
            if (order is null)
                throw new NotFoundException(string.Format(Messages.NotFound, "Order"));

            var auditLogSpec = new AuditLogSpecification(x =>
                x.EntityName == nameof(Order) && x.EntityId == orderId.ToString()
                && (x.Action != AuditAction.Update || x.OldValues != null || x.NewValues != null));

            var totalCount = await _unitOfWork.Repository<AuditLog>().CountAsync(auditLogSpec);

            auditLogSpec.ApplyPagination((dto.PageSize * (dto.PageIndex - 1)), dto.PageSize);
            var auditLogs = await _unitOfWork.Repository<AuditLog>().ListAsync(auditLogSpec);
            var result = _mapper.Map<List<AuditLogDto>>(auditLogs);

            return new PaginationModel<AuditLogDto>(dto.PageIndex, dto.PageSize, totalCount, result);
        }

        public async Task<OperationResultModel> SendOrderInvoiceToUserAsync(int orderId)
        {
            var orderSpec = new OrderWithAllDetailsSpecification(x => x.Id == orderId);
            var order = await _unitOfWork.Repository<Order>().GetFirstOrDefaultAsync(orderSpec);
            if (order is null)
                throw new NotFoundException(string.Format(Messages.NotFound,"Order"));
            if (order.UserId != _currentUser.UserId)
                throw new BadRequestException(Messages.OrderNotBelongToUser);
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
                Message = Messages.InvoiceSentToEmail
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

        private async Task RestoreOrderStockAsync(Order order)
        {
            var productIds = order.Items.Where(x => x.ProductId != null)
                                         .Select(x => x.ProductId!.Value)
                                         .Distinct()
                                         .ToList();
            if (!productIds.Any())
                return;

            var productsSpec = new ProductWithAllDataSpecification(x => productIds.Contains(x.Id));
            var products = await _unitOfWork.Repository<Product>().ListAsync(productsSpec);

            foreach (var item in order.Items)
            {
                var product = products.FirstOrDefault(x => x.Id == item.ProductId);
                var stock = product?.Stocks.FirstOrDefault(x => x.Size == item.Size && x.StockType == item.Gender);
                if (stock is not null)
                    stock.StockQuantity += item.Quantity;
            }

            foreach (var product in products)
                product.IsOutOfStock = product.Stocks.Sum(x => x.StockQuantity) < 1;

            _unitOfWork.Repository<Product>().UpdateRange(products);
        }

        private OrderDetailsDto MapToOrderDetailsDto(Order order)
        {
            return new OrderDetailsDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                CustomerName = order.User?.FullName ?? string.Empty,
                CustomerEmail = order.User?.Email ?? string.Empty,
                Status = order.Status,
                Subtotal = order.Subtotal,
                ShippingCost = order.ShippingCost,
                DiscountAmount = order.DiscountAmount,
                TotalAmount = order.TotalAmount,
                HasCoupon = order.HasCoupon,
                CouponCode = order.CouponCode,
                CouponDiscountAmount = order.CouponDiscountAmount,
                HasLegendCard = order.HasLegendCard,
                LegendCardPrice = order.LegendCardPrice,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                Shipping = new OrderShippingDto
                {
                    RecipientName = order.Shipping.RecipientName,
                    Country = order.Shipping.Country,
                    Governorate = order.Shipping.Governorate,
                    Area = order.Shipping.Area,
                    DetailedAddress = order.Shipping.DetailedAddress,
                    PhoneNumber = order.Shipping.PhoneNumber,
                    Notes = order.Shipping.Notes,
                    EstimatedDeliveryDate = order.Shipping.EstimatedDeliveryDate,
                    DeliveredAt = order.Shipping.DeliveredAt
                },
                Payment = new OrderPaymentDto
                {
                    Status = order.Payment.Status,
                    Amount = order.Payment.Amount,
                    PaidAt = order.Payment.PaidAt
                },
                Items = order.Items.Select(item => new OrderItemDto
                {
                    ProductId = item.ProductId,
                    ProductName = item.Translations.FirstOrDefault(x => x.Language == _currentLanguage.Language)?.ProductName
                                  ?? item.Translations.FirstOrDefault(x => x.Language == Language.English)?.ProductName
                                  ?? "Product",
                    ProductImageUrl = _fileService.GetFullUrl(item.ProductImageUrl),
                    Gender = item.Gender,
                    Size = item.Size,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.TotalPrice,
                    IsCustomPrint = item.IsCustomPrint,
                    PrintName = item.PrintName,
                    PrintNumber = item.PrintNumber
                }).ToList()
            };
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
