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
        public DiscKind LastChosenDiscKind { get; private set; } = DiscKind.Ordinary;

        public ImmediateWinOrRandomAIStrategy(IWinRule winRule, int aiPlayerId = 2)
        {
            _winRule = winRule;
            _aiPlayerId = aiPlayerId;
        }

        public int ChooseMove(Board board)
        {
            var aiPlayer = _aiPlayerId == 1 ? Player.Player1 : Player.Player2;
            var availableKinds = new List<DiscKind>();
            foreach (DiscKind kind in Enum.GetValues(typeof(DiscKind)))
            {
                if (aiPlayer.Inventory.TryGetValue(kind, out var cnt) && cnt > 0)
                    availableKinds.Add(kind);
            }

            // 1) try immediate winning moves using any available piece kind
            for (int c = 0; c < board.Cols; c++)
            {
                if (!board.IsLegalMove(c)) continue;
                int row = FirstEmptyRow(board, c);
                if (row < 0) continue;
                var cell = board.Cells[row][c];
                foreach (var kind in availableKinds)
                {
                    cell.Disc = DiscFactory.Create(kind, _aiPlayerId);
                    bool win = _winRule.CheckCellWin(board, cell);
                    cell.Disc = null; // revert
                    if (win)
                    {
                        LastChosenDiscKind = kind;
                        return c;
                    }
                }
            }

            // 2) fallback to random legal move
            var legal = new List<int>();
            for (int c = 0; c < board.Cols; c++)
            {
                if (board.IsLegalMove(c)) legal.Add(c);
            }
            if (legal.Count == 0) return -1;

            // 2.1) choose a piece kind for fallback: prefer Ordinary if available else first available
            if (aiPlayer.Inventory.TryGetValue(DiscKind.Ordinary, out var ordinaryCnt) && ordinaryCnt > 0)
            {
                LastChosenDiscKind = DiscKind.Ordinary;
            }
            else if (availableKinds.Count > 0)
            {
                LastChosenDiscKind = availableKinds[0];
            }
            else
            {
                // no stock at all, still pick a column; actual move will fail at consumption
                LastChosenDiscKind = DiscKind.Ordinary;
            }

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
