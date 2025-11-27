using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LotteryGame.Core.Interfaces
{
    public interface ILottery
    {
        decimal GetTotalRevenue();
        decimal GetHouseProfit();
        bool BuyTickets(Player player, int ticketCost, int numberOfTickets);
        List<Ticket>? GetTicketsForPlayer(Player player);
        LotteryDrawResult DrawPrizeWinners();
    }
}
