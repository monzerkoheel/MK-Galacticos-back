using MK.Core.HelperModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.Models
{
    public class FanRequestLike : ModelBase
    {
        public string UserId { get; set; } = null!;
        public int FanRequestId { get; set; }
        public DateTime CreatedAt { get; set; }


        public ApplicationUser User { get; set; } = null!;
        public FanRequest FanRequest { get; set; } = null!;
    }
}
