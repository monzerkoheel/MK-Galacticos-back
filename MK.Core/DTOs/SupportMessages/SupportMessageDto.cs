namespace MK.Core.DTOs.SupportMessages
{
    public class SupportMessageDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Message { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public string? UserId { get; set; }
        public bool IsHandled { get; set; }
        public string? HandledByUserId { get; set; }
        public string? HandledByName { get; set; }
        public DateTime? HandledAt { get; set; }
    }
}
