using LotteryGame.Core.Configurations;
using LotteryGame.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LotteryGame.Business
{
    public class LotteryGameFactory
    {
        public static LotteryOrchestrator CreateStandardGame()
        {
            var config = new LotteryConfig();
            var randomGenerator = new RandomGenerator();
            var ticketGenerator = new TicketGenerator(randomGenerator);

            var lottery = new Lottery(randomGenerator, config, ticketGenerator);

            return new LotteryOrchestrator(lottery, config);
        }

        public static LotteryOrchestrator CreateCustomGame(LotteryConfig config)
        {
            var randomGenerator = new RandomGenerator();
            var ticketGenerator = new TicketGenerator(randomGenerator);

            var lottery = new Lottery(randomGenerator, config, ticketGenerator);

            return new LotteryOrchestrator(lottery, config);
        }

        public static LotteryOrchestrator CreateTestGame(IRandomGenerator mockRandom, LotteryConfig config)
        {
            var lottery = new Lottery(mockRandom, config);
            return new LotteryOrchestrator(lottery, config);
        }
    }
}
