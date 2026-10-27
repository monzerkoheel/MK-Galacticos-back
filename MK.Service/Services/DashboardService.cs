using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MK.Core.Contracts.ServicesContracts;
using MK.Core.DTOs.Dashboard;
using MK.Core.HelperModels;
using MK.Core.HelperModels.Exceptions;
using MK.Core.Localization;
using MK.Core.Models;
using MK.Core.Specifications.SpecificationParameters;
using MK.Repository.Data;

namespace MK.Service.Services
{
    /// <summary>
    /// Read-only reporting service. It uses the DbContext directly with AsNoTracking projections
    /// (aggregates and small projections), instead of loading full entities through the repositories.
    /// </summary>
    public class DashboardService : IDashboardService
    {
        private const int ListSize = 5;
        private const int MaxRangeDays = 366;
        private const string CustomerRole = "Customer";

        // Orders with these statuses are not counted as revenue / orders.
        private static readonly OrderStatus[] NonRevenueStatuses =
        {
            OrderStatus.Cancelled,
            OrderStatus.NotReceived,
            OrderStatus.ReFunded
        };

        private readonly MKGalacticosDbContext _dbContext;
        private readonly IFileService _fileService;
        private readonly ICurrentUserService _currentUser;

        public DashboardService(MKGalacticosDbContext dbContext, IFileService fileService, ICurrentUserService currentUser)
        {
            _dbContext = dbContext;
            _fileService = fileService;
            _currentUser = currentUser;
        }

        public async Task<DashboardDto> GetDashboardAsync(DashboardRangePrams prams)
        {
            var (from, toExclusive) = ResolveRange(prams);
            var days = (toExclusive - from).Days;
            var previousFrom = from.AddDays(-days);

            var current = await GetOrderTotalsAsync(from, toExclusive);
            var previous = await GetOrderTotalsAsync(previousFrom, from);

            var result = new DashboardDto
            {
                Period = new DashboardPeriodDto
                {
                    From = from,
                    To = toExclusive.AddDays(-1),
                    PreviousFrom = previousFrom,
                    PreviousTo = from.AddDays(-1)
                },
                Kpis = await BuildKpisAsync(from, toExclusive, previousFrom, current, previous),
                RevenueBreakdown = BuildRevenueBreakdown(current),
                RecentOrders = await GetRecentOrdersAsync(),
                LatestFanRequests = await GetLatestFanRequestsAsync(),
                LowStockProducts = await GetLowStockProductsAsync()
            };
            var role = _currentUser.Role;
            if (role != "Admin" && role != "Owner" && role != "Manager")
                result.Kpis.TotalRevenue = new DashboardMetricDto { Value = 0, ChangePercent = null };
            return result;
        }

        public async Task<SalesOverviewDto> GetSalesOverviewAsync(SalesOverviewRange range)
        {
            var today = DateTime.UtcNow.Date;
            var result = new SalesOverviewDto { Range = range };

            if (range == SalesOverviewRange.Last12Months)
            {
                var currentMonth = new DateTime(today.Year, today.Month, 1);
                var firstMonth = currentMonth.AddMonths(-11);
                var end = currentMonth.AddMonths(1);

                var grouped = await RevenueOrders()
                    .Where(x => x.CreatedAt >= firstMonth && x.CreatedAt < end)
                    .GroupBy(x => new { x.CreatedAt.Year, x.CreatedAt.Month })
                    .Select(g => new
                    {
                        g.Key.Year,
                        g.Key.Month,
                        Revenue = g.Sum(x => x.TotalAmount),
                        Orders = g.Count()
                    })
                    .ToListAsync();

                var lookup = grouped.ToDictionary(x => (x.Year, x.Month));

                for (var i = 0; i < 12; i++)
                {
                    var month = firstMonth.AddMonths(i);
                    lookup.TryGetValue((month.Year, month.Month), out var item);

                    result.Points.Add(new SalesOverviewPointDto
                    {
                        Date = month,
                        Revenue = item?.Revenue ?? 0,
                        Orders = item?.Orders ?? 0
                    });
                }

                return result;
            }

            var daysCount = range == SalesOverviewRange.Last30Days ? 30 : 7;
            var firstDay = today.AddDays(-(daysCount - 1));
            var endDay = today.AddDays(1);

            var perDay = await RevenueOrders()
                .Where(x => x.CreatedAt >= firstDay && x.CreatedAt < endDay)
                .GroupBy(x => x.CreatedAt.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Revenue = g.Sum(x => x.TotalAmount),
                    Orders = g.Count()
                })
                .ToListAsync();

            var perDayLookup = perDay.ToDictionary(x => x.Date.Date);

            for (var i = 0; i < daysCount; i++)
            {
                var day = firstDay.AddDays(i);
                perDayLookup.TryGetValue(day, out var item);

                result.Points.Add(new SalesOverviewPointDto
                {
                    Date = day,
                    Revenue = item?.Revenue ?? 0,
                    Orders = item?.Orders ?? 0
                });
            }
            
            return result;
        }

        #region KPIs

        private async Task<DashboardKpisDto> BuildKpisAsync(
            DateTime from, DateTime toExclusive, DateTime previousFrom,
            OrderTotals current, OrderTotals previous)
        {
            // ---- Customers / Products / Fan Requests: total up to the end of the period vs total at its start ----
            var customers = Customers();
            var customersNow = await customers.CountAsync(x => x.CreatedAt < toExclusive);
            var customersBefore = await customers.CountAsync(x => x.CreatedAt < from);

            var products = _dbContext.Set<Product>().AsNoTracking();
            var productsNow = await products.CountAsync(x => x.CreatedAt < toExclusive);
            var productsBefore = await products.CountAsync(x => x.CreatedAt < from);

            var fanRequests = _dbContext.Set<FanRequest>().AsNoTracking();
            var fanRequestsNow = await fanRequests.CountAsync(x => x.CreatedAt < toExclusive);
            var fanRequestsBefore = await fanRequests.CountAsync(x => x.CreatedAt < from);

            // ---- Pending orders ----
            var orders = _dbContext.Set<Order>().AsNoTracking();
            var pendingNow = await orders.CountAsync(x => x.Status == OrderStatus.Pending);
            var pendingPlacedCurrent = await orders.CountAsync(x =>
                x.Status == OrderStatus.Pending && x.CreatedAt >= from && x.CreatedAt < toExclusive);
            var pendingPlacedPrevious = await orders.CountAsync(x =>
                x.Status == OrderStatus.Pending && x.CreatedAt >= previousFrom && x.CreatedAt < from);

            // ---- Coupons ----
            var activeCoupons = _dbContext.Set<Coupon>().AsNoTracking()
                .Where(x => x.UserVisibility == Visibility.Active);
            var activeCouponsCount = await activeCoupons.CountAsync();
            var newCouponsCount = await activeCoupons.CountAsync(x => x.CreatedAt >= from && x.CreatedAt < toExclusive);

            // ---- Low stock (same rule as ProductService.GetProductsStatisticsAsync, but counted in the database) ----
            var lowStockCount = await products.CountAsync(x =>
                !x.IsOutOfStock &&
                x.Stocks.Sum(s => s.StockQuantity) > 0 &&
                x.Stocks.Sum(s => s.StockQuantity) <= x.LowStockThreshold);

            return new DashboardKpisDto
            {
                TotalRevenue = Metric(current.Revenue, previous.Revenue),
                TotalOrders = Metric(current.Count, previous.Count),
                TotalCustomers = Metric(customersNow, customersBefore),
                TotalProducts = Metric(productsNow, productsBefore),
                FanRequests = Metric(fanRequestsNow, fanRequestsBefore),
                PendingOrders = new DashboardMetricDto
                {
                    Value = pendingNow,
                    ChangePercent = PercentChange(pendingPlacedCurrent, pendingPlacedPrevious)
                },
                ActiveCoupons = new DashboardActiveCouponsDto
                {
                    Value = activeCouponsCount,
                    NewInPeriod = newCouponsCount
                },
                LowStockProducts = lowStockCount
            };
        }

        private IQueryable<ApplicationUser> Customers()
        {
            // Local variables on purpose: EF Core inlines them as sub-queries.
            var userRoles = _dbContext.Set<IdentityUserRole<string>>();
            var customerRoleIds = _dbContext.Set<IdentityRole>()
                .Where(r => r.Name == CustomerRole)
                .Select(r => r.Id);

            return _dbContext.Set<ApplicationUser>()
                .AsNoTracking()
                .Where(u => u.Status != UserStatus.Deleted &&
                            userRoles.Any(ur => ur.UserId == u.Id && customerRoleIds.Contains(ur.RoleId)));
        }

        #endregion

        #region Revenue

        private IQueryable<Order> RevenueOrders()
            => _dbContext.Set<Order>()
                .AsNoTracking()
                .Where(x => !NonRevenueStatuses.Contains(x.Status));

        private sealed record OrderTotals(decimal Revenue, decimal Shipping, decimal LegendCards, decimal Discounts, int Count);

        private async Task<OrderTotals> GetOrderTotalsAsync(DateTime from, DateTime toExclusive)
        {
            var totals = await RevenueOrders()
                .Where(x => x.CreatedAt >= from && x.CreatedAt < toExclusive)
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    Revenue = g.Sum(x => x.TotalAmount),
                    Shipping = g.Sum(x => x.ShippingCost),
                    LegendCards = g.Sum(x => x.HasLegendCard ? (x.LegendCardPrice ?? 0) : 0),
                    Discounts = g.Sum(x => x.DiscountAmount ?? 0),
                    Count = g.Count()
                })
                .FirstOrDefaultAsync();

            return totals is null
                ? new OrderTotals(0, 0, 0, 0, 0)
                : new OrderTotals(totals.Revenue, totals.Shipping, totals.LegendCards, totals.Discounts, totals.Count);
        }

        /// <summary>
        /// Assumes Order.TotalAmount = Subtotal + ShippingCost + LegendCardPrice - discounts.
        /// Product sales is what is left of the revenue after shipping and legend cards, so the slices
        /// always add up to the total revenue.
        /// </summary>
        private static RevenueBreakdownDto BuildRevenueBreakdown(OrderTotals totals)
        {
            var productSales = Math.Max(0, totals.Revenue - totals.Shipping - totals.LegendCards);
            var itemsTotal = productSales + totals.Shipping + totals.LegendCards;

            return new RevenueBreakdownDto
            {
                TotalRevenue = totals.Revenue,
                TotalDiscounts = totals.Discounts,
                Items = new List<RevenueBreakdownItemDto>
                {
                    BreakdownItem("Product Sales", productSales, itemsTotal),
                    BreakdownItem("Shipping Fees", totals.Shipping, itemsTotal),
                    BreakdownItem("Legend Cards", totals.LegendCards, itemsTotal)
                }
            };
        }

        private static RevenueBreakdownItemDto BreakdownItem(string name, decimal amount, decimal total)
            => new()
            {
                Name = name,
                Amount = amount,
                Percentage = total == 0 ? 0 : Math.Round(amount / total * 100m, 1)
            };

        #endregion

        #region Lists

        private async Task<List<DashboardRecentOrderDto>> GetRecentOrdersAsync()
        {
            var rows = await _dbContext.Set<Order>()
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .ThenByDescending(x => x.Id)
                .Take(ListSize)
                .Select(x => new
                {
                    x.Id,
                    x.OrderNumber,
                    x.CreatedAt,
                    x.Status,
                    x.TotalAmount,
                    CustomerName = x.User.FullName,
                    CustomerImage = x.User.ProfileImageUrl
                })
                .ToListAsync();

            return rows.Select(x => new DashboardRecentOrderDto
            {
                Id = x.Id,
                OrderNumber = x.OrderNumber,
                CustomerName = x.CustomerName,
                CustomerImageUrl = ToFullUrl(x.CustomerImage),
                CreatedAt = x.CreatedAt,
                Status = x.Status,
                TotalAmount = x.TotalAmount
            }).ToList();
        }

        private async Task<List<DashboardFanRequestDto>> GetLatestFanRequestsAsync()
        {
            var rows = await _dbContext.Set<FanRequest>()
                .AsNoTracking()
                .Where(x => x.Status == FanRequestStatus.Approved || x.Status == FanRequestStatus.ComingSoon)
                .OrderByDescending(x => x.CreatedAt)
                .ThenByDescending(x => x.Id)
                .Take(ListSize)
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.ImageUrl,
                    x.LikesCount,
                    x.Status,
                    x.CreatedAt,
                    UserName = x.User.FullName
                })
                .ToListAsync();

            return rows.Select(x => new DashboardFanRequestDto
            {
                Id = x.Id,
                Name = x.Name,
                ImageUrl = ToFullUrl(x.ImageUrl),
                LikesCount = x.LikesCount,
                Status = x.Status,
                UserName = x.UserName,
                CreatedAt = x.CreatedAt
            }).ToList();
        }

        private async Task<List<DashboardLowStockDto>> GetLowStockProductsAsync()
        {
            var rows = await _dbContext.Set<ProductStock>()
                .AsNoTracking()
                .Where(x => x.StockQuantity > 0 && x.StockQuantity <= x.Product.LowStockThreshold)
                .OrderBy(x => x.StockQuantity)
                .ThenBy(x => x.Id)
                .Take(ListSize)
                .Select(x => new
                {
                    x.ProductId,
                    x.Size,
                    x.StockType,
                    x.StockQuantity,
                    Name = x.Product.Translations
                        .Where(t => t.Language == Language.English)
                        .Select(t => t.Name)
                        .FirstOrDefault(),
                    Images = x.Product.Images
                        .OrderBy(i => i.DisplayOrder)
                        .Select(i => new { i.ImageUrl, i.IsMain, i.Gender })
                        .ToList()
                })
                .ToListAsync();

            return rows.Select(x =>
            {
                // Main image of the same gender as the stock row, then any main image, then the first image.
                var image = x.Images.Where(i => i.IsMain && i.Gender == x.StockType).Select(i => i.ImageUrl).FirstOrDefault()
                            ?? x.Images.Where(i => i.IsMain).Select(i => i.ImageUrl).FirstOrDefault()
                            ?? x.Images.Select(i => i.ImageUrl).FirstOrDefault();

                return new DashboardLowStockDto
                {
                    ProductId = x.ProductId,
                    ProductName = x.Name ?? string.Empty,
                    ImageUrl = ToFullUrl(image),
                    Size = x.Size,
                    Gender = x.StockType,
                    Quantity = x.StockQuantity
                };
            }).ToList();
        }

        #endregion

        #region Helpers

        private static (DateTime From, DateTime ToExclusive) ResolveRange(DashboardRangePrams prams)
        {
            var to = (prams.To ?? DateTime.UtcNow).Date;
            var from = (prams.From ?? to.AddDays(-6)).Date;

            if (from > to)
                throw new BadRequestException("'From' date must not be after 'To' date.");

            if ((to - from).Days + 1 > MaxRangeDays)
                throw new BadRequestException($"The selected period must not exceed {MaxRangeDays} days.");

            return (from, to.AddDays(1));
        }

        private static DashboardMetricDto Metric(decimal current, decimal previous)
            => new()
            {
                Value = current,
                ChangePercent = PercentChange(current, previous)
            };

        private static decimal? PercentChange(decimal current, decimal previous)
        {
            if (previous == 0)
                return current == 0 ? 0m : null;

            return Math.Round((current - previous) / previous * 100m, 1);
        }

        private string? ToFullUrl(string? url)
            => string.IsNullOrWhiteSpace(url) ? null : _fileService.GetFullUrl(url);

        #endregion
    }
}
