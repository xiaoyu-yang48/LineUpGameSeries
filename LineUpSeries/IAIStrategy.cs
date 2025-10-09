using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineUpSeries
{
    public interface IAIStrategy
    {
        int ChooseMove(Board board);
    }

    public sealed class RandomAIStrategy : IAIStrategy
    {
        private readonly Random _random = new Random();

        public int ChooseMove(Board board)
        {
            var legal = new List<int>();
            for (int c = 0; c < board.Cols; c++)
            {
                if (board.IsLegalMove(c)) legal.Add(c);
            }
            if (legal.Count == 0) return -1;
            return legal[_random.Next(legal.Count)];
        }
    }
}
