using LotteryGame.Business;
using LotteryGame.Core;

namespace LotteryGame.Tests
{
    public class Lotterytests
    {
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


        #endregion
    }
}