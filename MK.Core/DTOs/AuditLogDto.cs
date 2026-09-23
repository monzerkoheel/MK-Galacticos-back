using MK.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.DTOs
{
    public class AuditLogDto
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string? UserFullName { get; set; }
        public string? UserRole { get; set; }
        public AuditAction Action { get; set; }
        public AuditActorType ActorType { get; set; }
        public string EntityName { get; set; } = null!;
        public string? EntityId { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string? Description { get; set; }
        public string? IpAddress { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
