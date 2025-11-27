using LotteryGame.Core.Configurations;
using LotteryGame.Core.Interfaces;

namespace LotteryGame.Core.Strategies
{
    public interface IPrizeDistributionStrategy
    {
        List<WinningTicketResult> DrawWinners(
            IReadOnlyList<(Player Player, Ticket Ticket)> availableTickets,
            decimal totalRevenue,
            PrizeTierConfig config,
            IRandomGenerator random,
            Action<Player, Ticket> removeTicketCallback);
    }
}
