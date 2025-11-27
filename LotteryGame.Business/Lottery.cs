using LotteryGame.Business.Strategies;
using LotteryGame.Core;
using LotteryGame.Core.Configurations;
using LotteryGame.Core.Interfaces;
using LotteryGame.Core.Strategies;
using System.Collections.Concurrent;

namespace LotteryGame.Business
{
    public class Lottery : ILottery
    {
        private readonly ConcurrentDictionary<Player, List<Ticket>> _allTickets = new();
        private readonly IRandomGenerator _random;
        private readonly LotteryConfig _config;
        private readonly Dictionary<string, IPrizeDistributionStrategy> _strategies;
        private readonly ITicketGenerator? _ticketGenerator;
        private decimal _totalRevenue = 0;
        private decimal _houseProfit = 0;

        public Lottery() : this(new RandomGenerator(), new LotteryConfig()) { }
        public Lottery(IRandomGenerator random, LotteryConfig config, ITicketGenerator? ticketGenerator = null)
        {
            _random = random ?? throw new ArgumentNullException(nameof(random));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _ticketGenerator = ticketGenerator;

            _strategies = new Dictionary<string, IPrizeDistributionStrategy>
            {
                { _config.GrandPrize.TierName, new GrandPrizeStrategy() },
                { _config.SecondTier.TierName, new PercentagePrizeStrategy() },
                { _config.ThirdTier.TierName, new PercentagePrizeStrategy() }
            };
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
                if (_ticketGenerator != null)
                {
                    var newTickets = _ticketGenerator.GenerateTickets(numberOfTickets, _config);
                    tickets.AddRange(newTickets);
                }
                else
                {
                    for (int i = 0; i < numberOfTickets; i++)
                    {
                        var ticket = _random.Next(_config.MinTicketNumber, _config.MaxTicketNumber + 1).ToString();
                        tickets.Add(new Ticket { Number = ticket });
                    }
                }
            }
        }

        public LotteryDrawResult DrawPrizeWinners()
        {
            var winners = new List<WinningTicketResult>();

            var allTickets = GetAllTickets();

            if (allTickets.Count == 0)
                throw new InvalidOperationException("No tickets have been purchased.");

            var grandPrizeWinners = _strategies[_config.GrandPrize.TierName]
                .DrawWinners(allTickets, _totalRevenue, _config.GrandPrize, _random, RemoveTicketFromPool);
            winners.AddRange(grandPrizeWinners);

            //2nd Tier Winners (30% revenue for 10% of remaining tickets)
            var remainingTicketsAfterGrand = GetAllTickets();
            if (remainingTicketsAfterGrand.Any())
            {
                var secondTierWinners = _strategies[_config.SecondTier.TierName]
                    .DrawWinners(remainingTicketsAfterGrand, _totalRevenue, _config.SecondTier, _random, RemoveTicketFromPool);
                winners.AddRange(secondTierWinners);
            }

            //3rd Tier Winners (10%  revenue for 20% of remaining tickets)
            var remainingTicketsAfterSecond = GetAllTickets();
            if (remainingTicketsAfterSecond.Any())
            {
                var thirdTierWinners = _strategies[_config.ThirdTier.TierName]
                    .DrawWinners(remainingTicketsAfterSecond, _totalRevenue, _config.ThirdTier, _random, RemoveTicketFromPool);
                winners.AddRange(thirdTierWinners);
            }

            var totalPrizesAwarded = winners.Sum(w => w.PrizeAmount);
            _houseProfit = _totalRevenue - totalPrizesAwarded;

            return new LotteryDrawResult
            {
                Winners = winners,
                HouseProfitFromRounding = _houseProfit - (_totalRevenue * _config.HousePercentage),
                TotalPrizesAwarded = totalPrizesAwarded
            };
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

        private IReadOnlyList<(Player Player, Ticket Ticket)> GetAllTickets()
        {
            return _allTickets
                .SelectMany(kvp => kvp.Value.Select(ticket => (kvp.Key, ticket)))
                .ToList();
        }
    }
}
