using LotteryGame.Business;
using LotteryGame.Core;
using LotteryGame.Core.Configurations;
using LotteryGame.Core.Interfaces;

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
            var ticketsPerPlayer = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 5, 3 }; 

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
            // Arrange
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
                var ticketCount = (int)scenario.Revenue;

                for (int i = 0; i < ticketCount; i++)
                {
                    mockValues.Add(200 + i);
                }
                mockValues.Add(0);

                var config = new LotteryConfig();
                var lottery = new Lottery(new MockRandomProvider(mockValues.ToArray()), config);
                var player = new Player { Name = "Player1", Balance = 1000 };

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
            var mockValues = new List<int>();

            for (int i = 0; i < 30; i++)
            {
                mockValues.Add(300 + i);
            }

            mockValues.Add(0);
            mockValues.Add(5);
            mockValues.Add(10);

            var config = new LotteryConfig();
            var lottery = new Lottery(new MockRandomProvider(mockValues.ToArray()), config);
            var players = CreatePlayers(3);

            foreach (var player in players)
            {
                lottery.BuyTickets(player, ticketCost: 1, numberOfTickets: 10);
            }

            // Act
            var result = lottery.DrawPrizeWinners();

            // Assert
            var secondTierWinners = result.Winners.Skip(1).Where(w => w.PrizeAmount > 0).ToList();

            var expectedSecondTierPrize = Math.Floor(9m / 2 * 100) / 100;

            foreach (var winner in secondTierWinners.Take(2))
            {
                Assert.Equal(expectedSecondTierPrize, winner.PrizeAmount);
            }
        }

        [Fact]
        public void DrawPrizeWinners_ThirdTier_TenPercentOfRevenueSplit()
        {
            var mockValues = new List<int>();

            for (int i = 0; i < 50; i++)
            {
                mockValues.Add(400 + i);
            }

            mockValues.Add(0); 
            for (int i = 0; i < 4; i++)
            {
                mockValues.Add(i * 10);
            }

            var thirdTierIndices = new[] { 0, 5, 10, 15, 20, 25, 30, 35, 36 };
            foreach (var index in thirdTierIndices)
            {
                mockValues.Add(index);
            }

            var config = new LotteryConfig();
            var lottery = new Lottery(new MockRandomProvider(mockValues.ToArray()), config);
            var players = CreatePlayers(10);

            foreach (var player in players)
            {
                lottery.BuyTickets(player, ticketCost: 1, numberOfTickets: 5);
            }

            // Act
            var result = lottery.DrawPrizeWinners();

            // Assert
            var expectedThirdTierPrize = Math.Floor(5m / 9 * 100) / 100;

            var thirdTierWinners = result.Winners.Skip(5).Take(9).ToList();

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
            var config = new LotteryConfig();
            var lottery = new Lottery(new MockRandomProvider(mockValues), config);
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