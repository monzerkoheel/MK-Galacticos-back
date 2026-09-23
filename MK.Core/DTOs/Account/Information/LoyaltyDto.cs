using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.DTOs.Account.Information
{
    public class LoyaltyDto
    {
        public int Balance { get; set; }
        public int LifetimeEarnedPoints { get; set; }
        public string CurrentTier { get; set; } = string.Empty;
        public string? NextTier { get; set; }
        public int PointsToNextTier { get; set; }
        public List<LoyaltyTransactionDto> RecentTransactions { get; set; } = new List<LoyaltyTransactionDto>();
    }
}
