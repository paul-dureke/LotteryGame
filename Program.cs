// See https://aka.ms/new-console-template for more information
using LotteryGame.Business;
using LotteryGame.Core;
using System.Numerics;

Console.WriteLine("Welcome to the Bede Lottery, Player 1!");
Console.WriteLine();
Console.WriteLine("* Your digital balance: $10.00");
Console.WriteLine("* Ticket price: $1.00 each");
Console.WriteLine();

var lottery = new Lottery();
var players = new List<Player>();

//Create random 10-15 players
var random = new Random();
var playerCount = random.Next(10, 16);

//Human player (Player1)
var humanPlayer = new Player { Name = "Player1", Balance = 10 };
players.Add(humanPlayer);

Console.Write("How many tickets do you want to buy, Player1? "); 
Console.WriteLine();
if (int.TryParse(Console.ReadLine(), out int humanTickets) && humanTickets >= 1 && humanTickets <= 10)
{
    if (!lottery.BuyTickets(humanPlayer, ticketCost: 1, numberOfTickets: humanTickets))
    {
        Console.WriteLine("Insufficient funds!");
        return;
    }
}
else
{
    Console.WriteLine("Invalid input. Exiting...");
    return;
}

//CPU players
for (int i = 2; i <= playerCount; i++)
{
    var cpuPlayer = new Player { Name = $"Player{i}", Balance = 10 };
    players.Add(cpuPlayer);

    var ticketsToBuy = random.Next(1, 11);
    ticketsToBuy = Math.Min(ticketsToBuy, 10);

    lottery.BuyTickets(cpuPlayer, ticketCost: 1, numberOfTickets: ticketsToBuy);
}
Console.WriteLine();
Console.WriteLine($"{playerCount} other CPU players also have purchased tickets.");
Console.WriteLine();

//Draw winners
try
{
    var result = lottery.DrawPrizeWinners();

    //Display formatted results
    LotteryResultFormatter.DisplayResults(result, lottery.GetHouseProfit());
}
catch (Exception ex)
{
    Console.WriteLine($"Error during lottery draw: {ex.Message}");
}

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();
