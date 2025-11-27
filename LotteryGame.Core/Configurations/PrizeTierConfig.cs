using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LotteryGame.Core.Configurations
{
    public class PrizeTierConfig
    {
        public string TierName { get; set; } = string.Empty;
        public decimal PrizePercentage { get; set; }
        public decimal? WinnerPercentage { get; set; }
        public int? FixedWinnerCount { get; set; }
    }
}
