// See https://aka.ms/new-console-template for more information
using LotteryGame.Business;
using LotteryGame.Core;

try
{
    var gameController = LotteryGameFactory.CreateStandardGame();
    gameController.StartGame();
    gameController.DisplayExitMessage();
}
catch (Exception ex)
{
    Console.WriteLine($"Fatal error: {ex.Message}");
    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
}
