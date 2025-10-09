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
                if (board.IsColumnLegal(c)) legal.Add(c);
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

            // 1) try immediate winning moves using any available piece kind
            for (int c = 0; c < board.Cols; c++)
            {
                if (!board.IsColumnLegal(c)) continue;
                foreach (DiscKind kind in Enum.GetValues(typeof(DiscKind)))
                {
                    var disc = DiscFactory.Create(kind, _aiPlayerId);
                    if (!board.IsDiscLegal(disc)) continue;
                    var simBoard = board.Clone();
                    var move = new PlaceDiscMove(c, disc);
                    move.Execute(simBoard);
                    simBoard.ApplyGravity();
                    bool p1, p2;
                    ( _winRule as ConnectWinRule ?? new ConnectWinRule(4) )
                        .WinCheck(simBoard, move.ChangeCells, out p1, out p2);
                    bool win = (_aiPlayerId == 1 ? p1 : p2);
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
                if (board.IsColumnLegal(c)) legal.Add(c);
            }
            if (legal.Count == 0) return -1;

            // 2.1) choose a piece kind for fallback: prefer Ordinary if available else first available
            if (aiPlayer.Inventory.TryGetValue(DiscKind.Ordinary, out var ordinaryCnt) && ordinaryCnt > 0)
            {
                LastChosenDiscKind = DiscKind.Ordinary;
            }
            else
            {
                // choose first legal kind by IsDiscLegal
                bool chosen = false;
                foreach (DiscKind kind in Enum.GetValues(typeof(DiscKind)))
                {
                    var disc = DiscFactory.Create(kind, _aiPlayerId);
                    if (board.IsDiscLegal(disc))
                    {
                        LastChosenDiscKind = kind;
                        chosen = true;
                        break;
                    }
                }
                if (!chosen)
                {
                    // no stock at all, still pick a column; actual move will fail at consumption
                    LastChosenDiscKind = DiscKind.Ordinary;
                }
            }

            return legal[_random.Next(legal.Count)];
        }

        // no need to compute first empty row with simulation approach
    }
}
