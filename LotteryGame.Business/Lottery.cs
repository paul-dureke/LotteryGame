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

        public LotteryDrawResult DrawPrizeWinners()
        {
            var winners = new List<WinningTicketResult>();

            var allTickets = GetAllTickets();

            if (allTickets.Count == 0)
                throw new InvalidOperationException("No tickets have been purchased.");

            var grandPrizeWinner = DrawGrandPrizeWinner(allTickets);
            winners.Add(grandPrizeWinner);

            //2nd Tier Winners (30% revenue for 10% of remaining tickets)
            var secondTierWinners = DrawSecondTierWinners();
            winners.AddRange(secondTierWinners);

            //3rd Tier Winners (10%  revenue for 20% of remaining tickets)
            var thirdTierWinners = DrawThirdTierWinners();
            winners.AddRange(thirdTierWinners);

            var totalPrizesAwarded = winners.Sum(w => w.PrizeAmount);
            _houseProfit = _totalRevenue - totalPrizesAwarded;

            return new LotteryDrawResult
            {
                Winners = winners,
                HouseProfitFromRounding = _houseProfit - (_totalRevenue * 0.1m),
                TotalPrizesAwarded = totalPrizesAwarded
            };
        }

        private List<WinningTicketResult> DrawThirdTierWinners()
        {
            var winners = new List<WinningTicketResult>();
            var remainingTickets = GetAllTickets();

            if (remainingTickets.Count == 0)
                return winners;

            //20% of remaining tickets (minimum 1 if any tickets exist)
            var numberOfWinners = Math.Max(1, (int)Math.Floor(remainingTickets.Count * 0.2));

            //total 3rd tier prize pool (10% of total revenue)
            var totalThirdTierPrize = _totalRevenue * 0.1m;

            //Calculate prize per winner
            var prizePerWinner = Math.Floor(totalThirdTierPrize / numberOfWinners * 100) / 100;

            //Draw the winners
            for (int i = 0; i < numberOfWinners && remainingTickets.Count > 0; i++)
            {
                var index = _random.Next(0, remainingTickets.Count);
                var winner = remainingTickets[index];

                var winningResult = new WinningTicketResult(
                    PlayerNumber: winner.Player.Name ?? string.Empty,
                    TicketNumber: winner.Ticket.Number,
                    PrizeAmount: prizePerWinner);

                winners.Add(winningResult);

                //Remove winner befpre next iteration
                RemoveTicketFromPool(winner.Player, winner.Ticket);
                remainingTickets = GetAllTickets();
            }

            return winners;
        }

        private List<WinningTicketResult> DrawSecondTierWinners()
        {
            var winners = new List<WinningTicketResult>();
            var remainingTickets = GetAllTickets();

            if (remainingTickets.Count == 0)
                return winners;

            //10% of remaining tickets (minimum 1 if any tickets exist)
            var numberOfWinners = Math.Max(1, (int)Math.Floor(remainingTickets.Count * 0.1));

            //total 2nd tier prize pool (30% of total revenue)
            var totalSecondTierPrize = _totalRevenue * 0.3m;

            //Calculate prize per winner
            var prizePerWinner = Math.Floor(totalSecondTierPrize / numberOfWinners * 100) / 100;

            // Draw the winners
            for (int i = 0; i < numberOfWinners && remainingTickets.Count > 0; i++)
            {
                var index = _random.Next(0, remainingTickets.Count);
                var winner = remainingTickets[index];

                var winningResult = new WinningTicketResult(
                    PlayerNumber: winner.Player.Name ?? string.Empty,
                    TicketNumber: winner.Ticket.Number,
                    PrizeAmount: prizePerWinner);

                winners.Add(winningResult);

                //Remove winner before next iteration
                RemoveTicketFromPool(winner.Player, winner.Ticket);
                remainingTickets = GetAllTickets();
            }

            return winners;
        }

        private void RemoveTicketFromPool(string playerName, string ticketNumber)
        {
            var playerEntry = _allTickets.FirstOrDefault(kvp => kvp.Key.Name == playerName);
            if (playerEntry.Key != null)
            {
                RemoveTicketFromPool(playerEntry.Key, new Ticket { Number = ticketNumber });
            }
        }

        private void RemoveTicketFromPool(Player player, Ticket ticketToRemove)
        {
            if (_allTickets.TryGetValue(player, out var playerTickets))
            {
                lock (playerTickets)
                {
                    // Find and remove the specific ticket by number
                    var ticketIndex = playerTickets.FindIndex(t => t.Number == ticketToRemove.Number);
                    if (ticketIndex >= 0)
                    {
                        playerTickets.RemoveAt(ticketIndex);
                    }

                    if (playerTickets.Count == 0)
                    {
                        _allTickets.TryRemove(player, out _);
                    }
                }
            }
        }

        private WinningTicketResult DrawGrandPrizeWinner(IReadOnlyList<(Player Player, Ticket Ticket)> allTickets)
        {
            var index = _random.Next(0, allTickets.Count);
            var winner = allTickets[index];
            var prizeAmount = _totalRevenue * 0.5m;

            var winningResult = new WinningTicketResult(
                PlayerNumber: winner.Player.Name ?? string.Empty,
                TicketNumber: winner.Ticket.Number,
                PrizeAmount: prizeAmount);

            // Remove the winning ticket from the pool
            RemoveTicketFromPool(winner.Player, winner.Ticket);

            return winningResult;
        }

        private IReadOnlyList<(Player Player, Ticket Ticket)> GetAllTickets()
        {
            return _allTickets
                .SelectMany(kvp => kvp.Value.Select(ticket => (kvp.Key, ticket)))
                .ToList();
        }
    }
}
