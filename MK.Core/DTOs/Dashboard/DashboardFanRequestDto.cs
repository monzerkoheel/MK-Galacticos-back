using MK.Core.Models;

namespace MK.Core.DTOs.Dashboard
{
    public class DashboardFanRequestDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public int LikesCount { get; set; }

        /// <summary>Approved (shown as "Open" in the UI) or ComingSoon.</summary>
        public FanRequestStatus Status { get; set; }

        public string UserName { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
