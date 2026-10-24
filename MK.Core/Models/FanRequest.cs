using MK.Core.HelperModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.Models
{
    public enum FanRequestStatus
    {
        Processing,
        Pending,
        Approved,
        ComingSoon,
        AvailableOnSite,
        Rejected,
        RejectedByAi
    }
    public class FanRequest : ModelBase
    {
        public string UserId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public FanRequestStatus Status { get; set; }
        public int LikesCount { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? RejectedReason { get; set; }

        public ApplicationUser User { get; set; } = null!;
        public ICollection<FanRequestLike> Likes { get; set; } = new List<FanRequestLike>();
        public ICollection<AvailabilityNotification> AvailabilityNotifications { get; set; } = new List<AvailabilityNotification>();
    }
}
