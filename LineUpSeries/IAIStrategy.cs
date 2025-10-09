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

    // Immediate win else random (assumes AI is playerId provided, places ordinary discs)
    public sealed class ImmediateWinOrRandomAIStrategy : IAIStrategy
    {
        private readonly IWinRule _winRule;
        private readonly int _aiPlayerId;
        private readonly Random _random = new Random();

        public ImmediateWinOrRandomAIStrategy(IWinRule winRule, int aiPlayerId = 2)
        {
            _winRule = winRule;
            _aiPlayerId = aiPlayerId;
        }

        public int ChooseMove(Board board)
        {
            // 1) try immediate winning moves using ordinary disc simulation
            for (int c = 0; c < board.Cols; c++)
            {
                if (!board.IsLegalMove(c)) continue;
                int row = FirstEmptyRow(board, c);
                if (row < 0) continue;
                var cell = board.Cells[row][c];
                cell.Disc = DiscRegistry.CreateDisc(DiscKind.Ordinary, _aiPlayerId);
                bool win = _winRule.CheckCellWin(board, cell);
                cell.Disc = null; // revert
                if (win) return c;
            }

            // 2) fallback to random legal move
            var legal = new List<int>();
            for (int c = 0; c < board.Cols; c++)
            {
                if (board.IsLegalMove(c)) legal.Add(c);
            }
            if (legal.Count == 0) return -1;
            return legal[_random.Next(legal.Count)];
        }

        private static int FirstEmptyRow(Board board, int col)
        {
            for (int r = 0; r < board.Rows; r++)
            {
                if (board.Cells[r][col].Disc == null) return r;
            }
            return -1;
        }
    }
}
