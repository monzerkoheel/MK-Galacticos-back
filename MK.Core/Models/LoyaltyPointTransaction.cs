using MK.Core.HelperModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.Models
{
    public enum LoyaltyPointTransactionType
    {
        Earn,
        Redeem
    }
    public enum LoyaltyTransactionReason
    {
        Purchase,
        FanRequest,
        ProductReview,
        DailyLogin,
        Referral,
        RewardRedemption
    }
    public class LoyaltyPointTransaction : ModelBase
    {
        public string UserId { get; set; } = null!;
        public int Points { get; set; }
        public LoyaltyPointTransactionType Type { get; set; }
        public LoyaltyTransactionReason Reason { get; set; }
        public int? OrderId { get; set; }
        public int? FanRequestId { get; set; }
        public int? ProductId { get; set; }
        public int? CouponId { get; set; }
        public DateTime CreatedAt { get; set; }
        public ApplicationUser User { get; set; } = null!;

    }
}
