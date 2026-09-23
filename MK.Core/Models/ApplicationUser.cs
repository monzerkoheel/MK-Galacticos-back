using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.Models
{
    public enum UserStatus
    {
        Active,
        Inactive,
        Banned,
        Deleted
    }
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public string? ProfileImageUrl { get; set; }
        public UserStatus Status { get; set; }
        public int LoyaltyPointsBalance { get; set; }
        public int LifetimeEarnedLoyaltyPoints { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

        public ICollection<UserAddress> Addresses { get; set; } = new List<UserAddress>();
        public Wishlist? Wishlist { get; set; }
        public Cart? Cart { get; set; }
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<UserCoupon> UserCoupons { get; set; } = new List<UserCoupon>();
        public ICollection<LoyaltyPointTransaction> LoyaltyPointTransactions { get; set; } = new List<LoyaltyPointTransaction>();
        public ICollection<ProductRating> ProductRatings { get; set; } = new List<ProductRating>();
        public ICollection<AvailabilityNotification> AvailabilityNotifications { get; set; } = new List<AvailabilityNotification>();
        public ICollection<FanRequest> FanRequests { get; set; } = new List<FanRequest>();
        public ICollection<FanRequestLike> FanRequestLikes { get; set; } = new List<FanRequestLike>();
        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    }
}
