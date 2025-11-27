using LotteryGame.Business;
using LotteryGame.Core;

namespace LotteryGame.Tests
{
    public class Lotterytests
    {
        private class MockRandomProvider : IRandomGenerator
        {
            private readonly Queue<int> _values = new();

            public MockRandomProvider(params int[] values)
            {
                foreach (var v in values)
                    _values.Enqueue(v);
            }

            public int Next(int minValue, int maxValue)
            {
                if (_values.Count > 0)
                {
                    // Use the scripted value (e.g. for DrawWinner)
                    var value = _values.Dequeue();

                    //safety check to avoid out-of-range issues
                    if (value < minValue || value >= maxValue)
                        throw new InvalidOperationException($"Scripted random value {value} is out of range [{minValue}, {maxValue}).");

                    return value;
                }

                return minValue;
            }
        }

        [Fact]
        public void BuyTickets_WhenPlayerHasEnoughBalance_ReturnsSuccessful()
        {
            //Arrange
            var player = new Player { Balance = 10 };
            var lottery = new Lottery();
            var ticketCost = 1;
            var numberOfTickets = 5;

            // Act
            var result = lottery.BuyTickets(player, ticketCost, numberOfTickets);

            // Assert
            Assert.True(result);
            Assert.Equal(5, player.Balance);
            Assert.Equal(numberOfTickets, lottery.GetTicketsForPlayer(player)?.Count);
        }

        [Fact]
        public void BuyTickets_WhenPlayersBuyTickets_UniqueTicketNumbersAreGenerated()
        {
            //Arrange
            var player = new Player { Balance = 10 };
            var lottery = new Lottery();
            var ticketCost = 1;
            var numberOfTickets = 9;
            // Act
            var result = lottery.BuyTickets(player, ticketCost, numberOfTickets);
            // Assert
            Assert.True(result);
            var tickets = lottery.GetTicketsForPlayer(player);
            Assert.NotNull(tickets);
            Assert.Equal(numberOfTickets, tickets?.Count);
            var distinctTicketNumbers = tickets!.Select(t => t.Number).Distinct().ToList();
            Assert.Equal(numberOfTickets, distinctTicketNumbers.Count);
        }

        [Fact]
        public void BuyTickets_WhenPlayerDoesNotHaveEnoughBalance_ReturnsUnsuccessful()
        {
            //Arrange
            var player = new Player { Balance = 10 };
            var lottery = new Lottery();
            var ticketCost = 1;
            var numberOfTickets = 12;

            // Act
            var result = lottery.BuyTickets(player, ticketCost, numberOfTickets);

            // Assert
            Assert.False(result);
            Assert.Equal(10, player.Balance);
        }

        [Fact]
        public void BuyTickets_WhenPlayerIsNull_ThrowsArgumentNullException()
        {
            //Arrange
            Player player = null;
            var lottery = new Lottery();
            var ticketCost = 1;
            var numberOfTickets = 5;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => lottery.BuyTickets(player, ticketCost, numberOfTickets));
        }

        [Fact]
        public void Lottery_WithMinimumPlayers_10PlayersEachBuy1Ticket()
        {
            // Arrange
            var lottery = new Lottery();
            var players = CreatePlayers(10);

            foreach (var player in players)
            {
                var result = lottery.BuyTickets(player, ticketCost: 1, numberOfTickets: 1);
                Assert.True(result);
            }

            // Assert
            Assert.Equal(10m, lottery.GetTotalRevenue());
            foreach (var player in players)
            {
                Assert.Equal(9, player.Balance);
                Assert.Single(lottery.GetTicketsForPlayer(player));
            }
        }

        [Fact]
        public void Lottery_WithMaximumPlayers_15PlayersEachBuy10Tickets()
        {
            // Arrange
            var lottery = new Lottery();
            var players = CreatePlayers(15);

            foreach (var player in players)
            {
                var result = lottery.BuyTickets(player, ticketCost: 1, numberOfTickets: 10);
                Assert.True(result);
            }

            // Assert
            Assert.Equal(150m, lottery.GetTotalRevenue());
            foreach (var player in players)
            {
                Assert.Equal(0, player.Balance);
                Assert.Equal(10, lottery.GetTicketsForPlayer(player)?.Count);
            }
        }

        [Fact]
        public void Lottery_WithMixedTicketPurchases_RandomTicketsPerPlayer()
        {
            // Arrange
            var lottery = new Lottery();
            var players = CreatePlayers(12);
            var ticketsPerPlayer = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 5, 3 }; // Mixed purchases

            // Act
            for (int i = 0; i < players.Count; i++)
            {
                var result = lottery.BuyTickets(players[i], ticketCost: 1, numberOfTickets: ticketsPerPlayer[i]);
                Assert.True(result);
            }

            // Assert
            var expectedRevenue = ticketsPerPlayer.Sum();
            Assert.Equal(expectedRevenue, lottery.GetTotalRevenue());

            for (int i = 0; i < players.Count; i++)
            {
                Assert.Equal(10 - ticketsPerPlayer[i], players[i].Balance);
                Assert.Equal(ticketsPerPlayer[i], lottery.GetTicketsForPlayer(players[i])?.Count);
            }
        }

        [Fact]
        public void DrawPrizeWinners_GrandPrize_AlwaysFiftyPercentOfRevenue()
        {
            // Arrange - Test with different revenue amounts
            var testScenarios = new[]
            {
                new { Revenue = 100m, ExpectedGrandPrize = 50m },
                new { Revenue = 37m, ExpectedGrandPrize = 18.5m },
                new { Revenue = 200m, ExpectedGrandPrize = 100m },
                new { Revenue = 1m, ExpectedGrandPrize = 0.5m }
            };

            foreach (var scenario in testScenarios)
            {
                var mockValues = new List<int>();
                var ticketCount = (int)scenario.Revenue; // 1 ticket per dollar

                // Generate ticket numbers
                for (int i = 0; i < ticketCount; i++)
                {
                    mockValues.Add(200 + i);
                }
                mockValues.Add(0); // Grand prize winner (first ticket)

                var lottery = new Lottery(new MockRandomProvider(mockValues.ToArray()));
                var player = new Player { Name = "Player1", Balance = 1000 };

                // Buy tickets to generate the target revenue
                lottery.BuyTickets(player, ticketCost: 1, numberOfTickets: ticketCount);

                // Act
                var result = lottery.DrawPrizeWinners();

                // Assert
                Assert.Equal(scenario.ExpectedGrandPrize, result.Winners[0].PrizeAmount);
                Assert.Equal("Player1", result.Winners[0].PlayerNumber);
            }
        }

        [Fact]
        public void DrawPrizeWinners_SecondTier_ThirtyPercentOfRevenueSplit()
        {
            // Arrange - 30 tickets total, revenue = $30
            var mockValues = new List<int>();

            // 30 ticket numbers
            for (int i = 0; i < 30; i++)
            {
                mockValues.Add(300 + i);
            }

            mockValues.Add(0); // Grand prize winner (removes 1 ticket)
            // Remaining: 29 tickets, 10% of 29 = 2.9 → 2 second tier winners
            mockValues.Add(5); // First second tier winner
            mockValues.Add(10); // Second second tier winner

            var lottery = new Lottery(new MockRandomProvider(mockValues.ToArray()));
            var players = CreatePlayers(3);

            // Each player buys 10 tickets
            foreach (var player in players)
            {
                lottery.BuyTickets(player, ticketCost: 1, numberOfTickets: 10);
            }

            // Act
            var result = lottery.DrawPrizeWinners();

            // Assert
            var secondTierWinners = result.Winners.Skip(1).Where(w => w.PrizeAmount > 0).ToList();

            // Second tier pool: 30% of $30 = $9
            // 2 winners should get $4.50 each (rounded down to $4.50)
            var expectedSecondTierPrize = Math.Floor(9m / 2 * 100) / 100; // $4.50

            foreach (var winner in secondTierWinners.Take(2)) // First 2 should be second tier
            {
                Assert.Equal(expectedSecondTierPrize, winner.PrizeAmount);
            }
        }

        [Fact]
        public void DrawPrizeWinners_ThirdTier_TenPercentOfRevenueSplit()
        {
            // Arrange - 50 tickets total, revenue = $50
            var mockValues = new List<int>();

            // 50 ticket numbers
            for (int i = 0; i < 50; i++)
            {
                mockValues.Add(400 + i);
            }

            mockValues.Add(0); // Grand prize winner (removes 1 ticket)
                               // Remaining: 49 tickets, 10% of 49 = 4.9 → 4 second tier winners
            for (int i = 0; i < 4; i++)
            {
                mockValues.Add(i * 10); // Second tier winners
            }

            // Remaining after second tier: 45 tickets, 20% of 45 = 9 third tier winners
            // Need to account for decreasing ticket pool: 45, 44, 43, 42, 41, 40, 39, 38, 37
            var thirdTierIndices = new[] { 0, 5, 10, 15, 20, 25, 30, 35, 36 }; // Last value must be < 37
            foreach (var index in thirdTierIndices)
            {
                mockValues.Add(index);
            }

            var lottery = new Lottery(new MockRandomProvider(mockValues.ToArray()));
            var players = CreatePlayers(10);

            // Each player buys 5 tickets
            foreach (var player in players)
            {
                lottery.BuyTickets(player, ticketCost: 1, numberOfTickets: 5);
            }

            // Act
            var result = lottery.DrawPrizeWinners();

            // Assert
            // Third tier pool: 10% of $50 = $5
            // 9 winners should get $0.55 each (rounded down)
            var expectedThirdTierPrize = Math.Floor(5m / 9 * 100) / 100; // $0.55

            var thirdTierWinners = result.Winners.Skip(5).Take(9).ToList(); // Skip grand + 4 second tier

            foreach (var winner in thirdTierWinners)
            {
                Assert.Equal(expectedThirdTierPrize, winner.PrizeAmount);
            }
        }

        [Fact]
        public void Lottery_HouseProfitCalculation_AllRevenueAccountedFor()
        {
            // Arrange
            var mockValues = GenerateTicketNumbers(50).Concat(new[] { 0, 10, 20 }).ToArray();
            var lottery = new Lottery(new MockRandomProvider(mockValues));
            var players = CreatePlayers(10);

            foreach (var player in players)
            {
                lottery.BuyTickets(player, ticketCost: 1, numberOfTickets: 5);
            }

            // Act
            var result = lottery.DrawPrizeWinners();

            // Assert
            var totalRevenue = lottery.GetTotalRevenue();
            var totalPrizes = result.Winners.Sum(w => w.PrizeAmount);
            var houseProfit = lottery.GetHouseProfit();

            // Revenue should equal prizes + house profit
            Assert.Equal(totalRevenue, totalPrizes + houseProfit);
        }

        #region Helper Methods

        private List<Player> CreatePlayers(int count, int startingBalance = 10, int startingIndex = 1)
        {
            var players = new List<Player>();
            for (int i = startingIndex; i < startingIndex + count; i++)
            {
                players.Add(new Player
                {
                    Name = $"Player{i}",
                    Balance = startingBalance
                });
            }
            return players;
        }

        private int[] GenerateTicketNumbers(int count, int startNumber = 200)
        {
            return Enumerable.Range(startNumber, count).ToArray();
        }
        #endregion
    }
}