using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LotteryGame.Core
{
    public class Ticket
    {
        public string Number { get; set; }
    }

    public record WinningTicketResult(string PlayerNumber, string TicketNumber, decimal PrizeAmount);

    public class LotteryDrawResult
    {
        public List<WinningTicketResult> Winners { get; init; } = new();
        public decimal HouseProfitFromRounding { get; init; }
        public decimal TotalPrizesAwarded { get; init; }
    }

    public static class LotteryResultFormatter
    {
        public static void DisplayResults(LotteryDrawResult result, decimal houseProfit)
        {
            Console.WriteLine("Ticket Draw Result:");
            Console.WriteLine();

            //Group winners by tier based on prize amount
            var grandPrize = result.Winners.OrderByDescending(w => w.PrizeAmount).FirstOrDefault();
            var remainingWinners = result.Winners.Where(w => w != grandPrize).ToList();

            if (grandPrize != null)
            {
                Console.WriteLine($"* Grand Prize: {grandPrize.PlayerNumber} (1) wins ${grandPrize.PrizeAmount:F2}!");
            }

            //Group remaining winners by prize amount 
            var tierGroups = remainingWinners
                .GroupBy(w => w.PrizeAmount)
                .OrderByDescending(g => g.Key)
                .ToList();

            //2nd tier
            if (tierGroups.Count > 0)
            {
                var secondTier = tierGroups[0];
                var secondTierPlayerCounts = GetPlayerWinningCounts(secondTier);
                var secondTierText = string.Join(", ", secondTierPlayerCounts.Select(kvp => $"{kvp.Key}({kvp.Value})"));
                Console.WriteLine($"* Second Tier: Players {secondTierText} win ${secondTier.Key:F2} per winning ticket!");
            }

            // 3rd tier
            if (tierGroups.Count > 1)
            {
                var thirdTier = tierGroups[1];
                var thirdTierPlayerCounts = GetPlayerWinningCounts(thirdTier);
                var thirdTierText = string.Join(", ", thirdTierPlayerCounts.Select(kvp => $"{kvp.Key}({kvp.Value})"));
                Console.WriteLine($"* Third Tier: Players {thirdTierText} win ${thirdTier.Key:F2} per winning ticket!");
            }

            Console.WriteLine();
            Console.WriteLine("Congratulations to the winnwers!");
            Console.WriteLine($"House Profit: ${houseProfit:F2}");
        }

        private static Dictionary<string, int> GetPlayerWinningCounts(IGrouping<decimal, WinningTicketResult> tierWinners)
        {
            return tierWinners
                .GroupBy(w => w.PlayerNumber)
                .ToDictionary(g => g.Key, g => g.Count())
                .OrderBy(kvp => ExtractPlayerNumber(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        private static int ExtractPlayerNumber(string playerName)
        {
            var numberPart = playerName.Replace("Player", "");
            return int.TryParse(numberPart, out var number) ? number : 0;
        }
    }
}
