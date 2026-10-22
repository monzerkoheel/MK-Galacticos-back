using MK.Core.DTOs.Orders;
using MK.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.DTOs.Account.Information
{
    public class UserProfileDto
    {
        public string UserId { get; set; } = null!;
        public string? ProfileImageUrl { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public LoyaltyDto UserLoyaltyData { get; set; }
        public int OrdersCount { get; set; }
        public int WishlistItemsCount { get; set; }
        public int FanRequestsCount { get; set; }
        public ICollection<UserAddressDto> Addresses { get; set; }
        public ICollection<OrderDto> Orders { get; set; } = new List<OrderDto>();
        public ICollection<UserCouponDto> UserCoupons { get; set; }
    }
}
