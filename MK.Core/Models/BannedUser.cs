using MK.Core.HelperModels;

namespace MK.Core.Models
{
    public class BannedUser : ModelBase
    {
        public string UserId { get; set; } = null!;

        public DateTime BannedAt { get; set; }
        public DateTime? BannedUntil { get; set; }
        public string? BanReason { get; set; }

        public ApplicationUser User { get; set; } = null!;
    }
}
