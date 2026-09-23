using MK.Core.HelperModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.Models
{
    public enum AuditAction
    {
        Create,
        Update,
        Delete,
        Login,
        Logout
    }
    public enum AuditActorType
    {
        User,
        Management,
        System
    }
    public class AuditLog : ModelBase
    {
        public string? UserId { get; set; }
        public AuditAction Action { get; set; }
        public AuditActorType ActorType { get; set; }
        public string EntityName { get; set; } = null!;
        public string? EntityId { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string? Description { get; set; }
        public string? IpAddress { get; set; }
        public DateTime CreatedAt { get; set; }
        public ApplicationUser? User { get; set; }
    }
}
