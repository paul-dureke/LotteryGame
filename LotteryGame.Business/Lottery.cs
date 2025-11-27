using LotteryGame.Core;
using System.Collections.Concurrent;

namespace LotteryGame.Business
{
    public class Lottery
    {
        private readonly ConcurrentDictionary<Player, List<Ticket>> _allTickets = new();
        private readonly IRandomGenerator _random;
        private decimal _totalRevenue = 0;
        private decimal _houseProfit = 0;

        public Lottery() : this(new RandomGenerator()) { }
        public Lottery(IRandomGenerator random)
        {
            _random = random ?? throw new ArgumentNullException(nameof(random));
        }

        public decimal GetTotalRevenue() => _totalRevenue;
        public decimal GetHouseProfit() => _houseProfit;

        public bool BuyTickets(Player player, int ticketCost, int numberOfTickets)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            if (ticketCost * numberOfTickets > player.Balance)
                return false;

            //generate and assign tickets to player here
            GenerateTicketsForPlayer(player, numberOfTickets);
            var totalCost = ticketCost * numberOfTickets;
            player.Balance -= totalCost;

            _totalRevenue += totalCost;

            return true;
        }

        public List<Ticket>? GetTicketsForPlayer(Player player)
        {
            return _allTickets.TryGetValue(player, out var tickets) ? tickets : null;
        }

        private void GenerateTicketsForPlayer(Player player, int numberOfTickets)
        {
            var tickets = _allTickets.GetOrAdd(player, _ => new List<Ticket>());

            lock (tickets)
            {
                for (int i = 0; i < numberOfTickets; i++)
                {
                    var ticket = _random.Next(111, 999).ToString();
                    tickets.Add(new Ticket { Number = ticket });
                }
            }
        }
    }
}
