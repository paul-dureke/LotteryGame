
namespace LotteryGame.Core.Interfaces
{
    public interface IRandomGenerator
    {
        int Next(int minValue, int maxValue);
    }
}
