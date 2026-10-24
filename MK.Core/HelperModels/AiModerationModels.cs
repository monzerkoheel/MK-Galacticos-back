namespace MK.Core.HelperModels
{
    public class AiModerationResult
    {
        public bool IsApproved { get; set; }
        public string? RejectionReason { get; set; }
        public string? ReasonCode { get; set; }
    }
    public record FanRequestModerationJob(int FanRequestId, string? Lang);
}
