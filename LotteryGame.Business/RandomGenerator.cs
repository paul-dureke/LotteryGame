
using LotteryGame.Core.Interfaces;

namespace LotteryGame.Business
{
    public class RandomGenerator : IRandomGenerator
    {
        private readonly Random _random = new();
        public int Next(int minValue, int maxValue)
        {
            return _random.Next(minValue, maxValue);
        }
    }
}
