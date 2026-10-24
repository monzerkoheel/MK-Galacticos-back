using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MK.Core.Models;

namespace MK.Repository.Data;

public class MKGalacticosDbContext : IdentityDbContext<ApplicationUser>
{
    public MKGalacticosDbContext(DbContextOptions<MKGalacticosDbContext> options): base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(MKGalacticosDbContext).Assembly);
    }

    public DbSet<Country> Countries => Set<Country>();
    public DbSet<Governorate> Governorates => Set<Governorate>();
    public DbSet<UserAddress> UserAddresses => Set<UserAddress>();

 
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Collection> Collections => Set<Collection>();
    public DbSet<ProductCollection> ProductCollections => Set<ProductCollection>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductStock> ProductStocks => Set<ProductStock>();
    public DbSet<ProductPrint> ProductPrints => Set<ProductPrint>();
    public DbSet<Wishlist> Wishlists => Set<Wishlist>();
    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<ShippingMethod> ShippingMethods => Set<ShippingMethod>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderShipping> OrderShippings => Set<OrderShipping>();
    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<UserCoupon> UserCoupons => Set<UserCoupon>();
    public DbSet<LoyaltyPointTransaction> LoyaltyPointTransactions => Set<LoyaltyPointTransaction>();
    public DbSet<LegendCard> LegendCards => Set<LegendCard>();
    public DbSet<ProductRating> ProductRatings => Set<ProductRating>();
    public DbSet<AvailabilityNotification> AvailabilityNotifications => Set<AvailabilityNotification>();
    public DbSet<FanRequest> FanRequests => Set<FanRequest>();
    public DbSet<FanRequestLike> FanRequestLikes => Set<FanRequestLike>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DbSet<DeletedAccount> DeletedAccounts => Set<DeletedAccount>();
    public DbSet<BannedUser> BannedUsers => Set<BannedUser>();


    // Translations
    public DbSet<CollectionTranslation> CollectionTranslations => Set<CollectionTranslation>();
    public DbSet<CouponTranslation> CouponTranslations => Set<CouponTranslation>();
    public DbSet<OrderItemTranslation> OrderItemTranslations => Set<OrderItemTranslation>();
    public DbSet<PaymentMethodTranslation> PaymentMethodTranslations => Set<PaymentMethodTranslation>();
    public DbSet<ProductTranslation> ProductTranslations => Set<ProductTranslation>();
    public DbSet<ShippingMethodTranslation> ShippingMethodTranslations => Set<ShippingMethodTranslation>();

    public DbSet<SupportMessage> SupportMessages => Set<SupportMessage>();
}