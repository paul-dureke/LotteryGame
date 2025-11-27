using LotteryGame.Core.Configurations;

namespace LotteryGame.Core.Interfaces
{
    public interface ITicketGenerator
    {
        IEnumerable<Ticket> GenerateTickets(int numberOfTickets, LotteryConfig config);
        Ticket GenerateTicket(LotteryConfig config);
        bool IsValidTicketNumber(string ticketNumber, LotteryConfig config);
    }
}
