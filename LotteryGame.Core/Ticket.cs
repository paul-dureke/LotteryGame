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
}
