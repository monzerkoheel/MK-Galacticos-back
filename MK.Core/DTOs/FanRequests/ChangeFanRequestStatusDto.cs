using MK.Core.Models;

namespace MK.Core.DTOs.FanRequests
{
    public class ChangeFanRequestStatusDto
    {
        public FanRequestStatus NewStatus { get; set; }
        public string? RejectedReason { get; set; }
    }
}
