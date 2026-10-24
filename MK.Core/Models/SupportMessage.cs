using MK.Core.HelperModels;

namespace MK.Core.Models
{
    public class SupportMessage : ModelBase
    {
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string? UserId { get; set; }
        public bool IsHandled { get; set; }
        public string? HandledByUserId { get; set; }
        public DateTime? HandledAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public byte[] RowVersion { get; set; } = null!;

        public ApplicationUser? User { get; set; }
        public ApplicationUser? HandledByUser { get; set; }
    }
}
