using MK.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.DTOs.Coupons
{
    public class CouponAuditDto
    {
        public string UserId { get; set; } = null!;
        public AuditAction Action { get; set; }
        public AuditActorType ActorType { get; set; }
        public string EntityName { get; set; } = null!;
        public string EntityId { get; set; } = null!;
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string? Description { get; set; }
        public string? IpAddress { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
