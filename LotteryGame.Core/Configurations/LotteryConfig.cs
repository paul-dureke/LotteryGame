
namespace LotteryGame.Core.Configurations
{
    public class LotteryConfig
    {
        public PrizeTierConfig GrandPrize { get; set; } = new()
        {
            TierName = "Grand Prize",
            PrizePercentage = 0.5m,
            WinnerPercentage = null,
            FixedWinnerCount = 1
        };

        public PrizeTierConfig SecondTier { get; set; } = new()
        {
            TierName = "Second Tier",
            PrizePercentage = 0.3m,
            WinnerPercentage = 0.1m
        };

        public PrizeTierConfig ThirdTier { get; set; } = new()
        {
            TierName = "Third Tier",
            PrizePercentage = 0.1m,
            WinnerPercentage = 0.2m
        };

        public decimal HousePercentage { get; set; } = 0.1m;
        public int MinTicketNumber { get; set; } = 111;
        public int MaxTicketNumber { get; set; } = 999;
        public int DefaultPlayerBalance { get; set; } = 10;
        public int DefaultTicketCost { get; set; } = 1;
        public int MinPlayers { get; set; } = 10;
        public int MaxPlayers { get; set; } = 15;
    }
}
