using LotteryGame.Core;
using LotteryGame.Core.Configurations;

namespace LotteryGame.Business
{
    public class LotteryResultFormatter
    {
        private readonly LotteryConfig _config;

        public LotteryResultFormatter(LotteryConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public void DisplayResults(LotteryDrawResult result, decimal houseProfit)
        {
            Console.WriteLine("Ticket Draw Result:");
            Console.WriteLine();

            //Group winners based on prize amount
            var grandPrize = result.Winners.OrderByDescending(w => w.PrizeAmount).FirstOrDefault();
            var remainingWinners = result.Winners.Where(w => w != grandPrize).ToList();

            if (grandPrize != null)
            {
                Console.WriteLine($"* {_config.GrandPrize.TierName}: {grandPrize.PlayerNumber} (1) wins ${grandPrize.PrizeAmount:F2}!");
            }

            var tierGroups = remainingWinners
                .GroupBy(w => w.PrizeAmount)
                .OrderByDescending(g => g.Key)
                .ToList();

            if (tierGroups.Count > 0)
            {
                //2nd tier
                var secondTier = tierGroups[0];
                var secondTierPlayerCounts = GetPlayerWinningCounts(secondTier, excludePlayerPrefix: true);
                var secondTierText = string.Join(", ", secondTierPlayerCounts.Select(kvp => $"{kvp.Key}({kvp.Value})"));
                Console.WriteLine($"* {_config.SecondTier.TierName}: Players {secondTierText} win ${secondTier.Key:F2} per winning ticket!");
            }

            if (tierGroups.Count > 1)
            {
                //3rd tier
                var thirdTier = tierGroups[1];
                var thirdTierPlayerCounts = GetPlayerWinningCounts(thirdTier, excludePlayerPrefix: true);
                var thirdTierText = string.Join(", ", thirdTierPlayerCounts.Select(kvp => $"{kvp.Key}({kvp.Value})"));
                Console.WriteLine($"* {_config.ThirdTier.TierName}: Players {thirdTierText} win ${thirdTier.Key:F2} per winning ticket!");
            }

            Console.WriteLine();
            Console.WriteLine("Congratulations to the winners!");
            Console.WriteLine($"House Profit: ${houseProfit:F2}");
        }

        private static Dictionary<string, int> GetPlayerWinningCounts(IGrouping<decimal, WinningTicketResult> tierWinners, bool excludePlayerPrefix = false)
        {
            return tierWinners
                .GroupBy(w => w.PlayerNumber)
                .ToDictionary(g => excludePlayerPrefix ? ExtractPlayerNumber(g.Key).ToString() : g.Key, g => g.Count())
                .OrderBy(kvp => ExtractPlayerNumber(excludePlayerPrefix ? $"Player{kvp.Key}" : kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        private static int ExtractPlayerNumber(string playerName)
        {
            var numberPart = playerName.Replace("Player", "");
            return int.TryParse(numberPart, out var number) ? number : 0;
        }
    }
}
