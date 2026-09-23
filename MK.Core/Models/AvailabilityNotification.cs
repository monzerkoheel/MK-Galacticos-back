using MK.Core.HelperModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.Models
{
    public enum AvailabilityNotificationType
    {
        ReStockProduct,
        FanRequestPublished
    }
    public class AvailabilityNotification : ModelBase
    {
        public string UserId { get; set; } = null!;
        public AvailabilityNotificationType Type { get; set; }

        public int? ProductId { get; set; }
        public ProductSize? ProductSize { get; set; }
        public int? FanRequestId { get; set; }
        public string Message { get; set; } = null!;
        public bool IsSent { get; set; }
        public DateTime? SentAt { get; set; }
        public DateTime CreatedAt { get; set; }


        public ApplicationUser User { get; set; } = null!;
        public Product? Product { get; set; }
        public FanRequest? FanRequest { get; set; }
    }
}
