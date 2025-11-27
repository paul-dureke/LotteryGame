using LotteryGame.Core;
using LotteryGame.Core.Configurations;
using LotteryGame.Core.Interfaces;

namespace LotteryGame.Business
{
    public class LotteryOrchestrator
    {
        private readonly ILottery _lottery;
        private readonly LotteryConfig _config;
        private readonly LotteryResultFormatter _formatter;
        private readonly List<Player> _players = new();
        private readonly Random _random = new();

        public LotteryOrchestrator(ILottery lottery, LotteryConfig config)
        {
            _lottery = lottery ?? throw new ArgumentNullException(nameof(lottery));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _formatter = new LotteryResultFormatter(config);
        }

        public void StartGame()
        {
            DisplayWelcomeMessage();

            var humanPlayer = CreateHumanPlayer();
            if (!ProcessHumanTicketPurchase(humanPlayer))
                return;

            CreateAndProcessCpuPlayers();
            DisplayGameStatus();

            RunLotteryDraw();
        }

        private void DisplayWelcomeMessage()
        {
            Console.WriteLine("Welcome to the Bede Lottery, Player 1!");
            Console.WriteLine();
            Console.WriteLine($"* Your digital balance: ${_config.DefaultPlayerBalance:F2}");
            Console.WriteLine($"* Ticket price: ${_config.DefaultTicketCost:F2} each");
            Console.WriteLine();
        }

        private Player CreateHumanPlayer()
        {
            var humanPlayer = new Player
            {
                Name = "Player1",
                Balance = _config.DefaultPlayerBalance
            };
            _players.Add(humanPlayer);
            return humanPlayer;
        }

        private bool ProcessHumanTicketPurchase(Player humanPlayer)
        {
            Console.Write("How many tickets do you want to buy, Player1? ");

            if (!int.TryParse(Console.ReadLine(), out int humanTickets) ||
                humanTickets < 1 ||
                humanTickets > humanPlayer.Balance)
            {
                Console.WriteLine("Invalid input. Exiting...");
                return false;
            }

            if (!_lottery.BuyTickets(humanPlayer, _config.DefaultTicketCost, humanTickets))
            {
                Console.WriteLine("Insufficient funds!");
                return false;
            }

            Console.WriteLine($"Player1 purchased {humanTickets} tickets. Remaining balance: ${humanPlayer.Balance}");
            Console.WriteLine();
            return true;
        }

        private void CreateAndProcessCpuPlayers()
        {
            var playerCount = _random.Next(_config.MinPlayers, _config.MaxPlayers + 1);

            for (int i = 2; i <= playerCount; i++)
            {
                var cpuPlayer = new Player
                {
                    Name = $"Player{i}",
                    Balance = _config.DefaultPlayerBalance
                };
                _players.Add(cpuPlayer);

                var ticketsToBuy = _random.Next(1, Math.Min(11, cpuPlayer.Balance + 1));
                _lottery.BuyTickets(cpuPlayer, _config.DefaultTicketCost, ticketsToBuy);
            }
        }

        private void DisplayGameStatus()
        {
            var cpuPlayerCount = _players.Count - 1; // Exclude human player
            Console.WriteLine($"{cpuPlayerCount} other CPU players also have purchased tickets.");
            Console.WriteLine();
            Console.WriteLine($"Total Revenue: ${_lottery.GetTotalRevenue():F2}");
            Console.WriteLine("Drawing winners...");
            Console.WriteLine();
        }

        private void RunLotteryDraw()
        {
            try
            {
                var result = _lottery.DrawPrizeWinners();
                _formatter.DisplayResults(result, _lottery.GetHouseProfit());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during lottery draw: {ex.Message}");
                Console.WriteLine($"Details: {ex}");
            }
        }

        public void DisplayExitMessage()
        {
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
