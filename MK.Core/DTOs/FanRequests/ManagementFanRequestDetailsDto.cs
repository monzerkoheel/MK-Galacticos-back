using MK.Core.Models;

namespace MK.Core.DTOs.FanRequests
{
    public class ManagementFanRequestDetailsDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public FanRequestStatus Status { get; set; }
        public int LikesCount { get; set; }
        public string? RejectedReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public string UserId { get; set; } = null!;
        public string UserFullName { get; set; } = null!;
        public string? UserImageUrl { get; set; }
    }
}
