using LotteryGame.Core;
using LotteryGame.Core.Configurations;
using LotteryGame.Core.Interfaces;
using LotteryGame.Core.Strategies;

namespace LotteryGame.Business.Strategies
{
    public class GrandPrizeStrategy : IPrizeDistributionStrategy
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

            var numberOfWinners = config.FixedWinnerCount ?? 1;

            var totalPrize = totalRevenue * config.PrizePercentage;

            // For grand prize, the single winner gets the entire prize amount
            var prizePerWinner = totalPrize;

            for (int i = 0; i < numberOfWinners && availableTickets.Count > 0; i++)
            {
                var index = random.Next(0, availableTickets.Count);
                var winner = availableTickets[index];

                var winningResult = new WinningTicketResult(
                    PlayerNumber: winner.Player.Name ?? string.Empty,
                    TicketNumber: winner.Ticket.Number,
                    PrizeAmount: prizePerWinner);

                winners.Add(winningResult);

                // Remove the winning ticket from the pool
                removeTicketCallback(winner.Player, winner.Ticket);
            }

            return winners;
        }
    }
}
