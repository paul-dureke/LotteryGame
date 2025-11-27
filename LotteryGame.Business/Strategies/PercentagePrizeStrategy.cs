using LotteryGame.Core;
using LotteryGame.Core.Configurations;
using LotteryGame.Core.Interfaces;
using LotteryGame.Core.Strategies;

namespace LotteryGame.Business.Strategies
{
    public class PercentagePrizeStrategy : IPrizeDistributionStrategy
    {
        public List<WinningTicketResult> DrawWinners(
            IReadOnlyList<(Player Player, Ticket Ticket)> availableTickets,
            decimal totalRevenue,
            PrizeTierConfig config,
            IRandomGenerator random,
            Action<Player, Ticket> removeTicketCallback)
        {
            var winners = new List<WinningTicketResult>();

            if (!availableTickets.Any())
                return winners;

            //Compute number of winners based on % of remaining tickets
            var numberOfWinners = config.FixedWinnerCount ??
                Math.Max(1, (int)Math.Floor(availableTickets.Count * (config.WinnerPercentage ?? 0)));

            var totalTierPrize = totalRevenue * config.PrizePercentage;
            var prizePerWinner = Math.Floor(totalTierPrize / numberOfWinners * 100) / 100;
            var currentTickets = availableTickets.ToList();

            for (int i = 0; i < numberOfWinners && currentTickets.Count > 0; i++)
            {
                var index = random.Next(0, currentTickets.Count);
                var winner = currentTickets[index];

                var winningResult = new WinningTicketResult(
                    PlayerNumber: winner.Player.Name ?? string.Empty,
                    TicketNumber: winner.Ticket.Number,
                    PrizeAmount: prizePerWinner);

                winners.Add(winningResult);

                removeTicketCallback(winner.Player, winner.Ticket);
                currentTickets.RemoveAt(index);
            }

            return winners;
        }
    }
}
