using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LotteryGame.Business
{
    public interface IRandomGenerator
    {
        int Next(int minValue, int maxValue);
    }

    public class RandomGenerator : IRandomGenerator
    {
        private readonly Random _random = new();
        public int Next(int minValue, int maxValue)
        {
            return _random.Next(minValue, maxValue);
        }
    }
}
