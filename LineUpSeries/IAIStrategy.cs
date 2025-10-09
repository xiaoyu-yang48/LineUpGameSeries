using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineUpSeries
{
    public interface IAIStrategy
    {
        PlaceDiscMove? PickMove(Board board);
    }

    public sealed class RandomAIStrategy : IAIStrategy
    {
        private readonly Random _random = new Random();

        public PlaceDiscMove? PickMove(Board board)
        {
            var legal = new List<int>();
            for (int c = 0; c < board.Cols; c++)
            {
                if (board.IsColumnLegal(c)) legal.Add(c);
            }
            if (legal.Count == 0) return null;
            int col = legal[_random.Next(legal.Count)];
            // choose first legal disc kind
            foreach (DiscKind kind in Enum.GetValues(typeof(DiscKind)))
            {
                var disc = DiscFactory.Create(kind, 2);
                if (board.IsDiscLegal(disc)) return new PlaceDiscMove(col, disc);
            }
            return null;
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

        public PlaceDiscMove? PickMove(Board board)
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
                        return new PlaceDiscMove(c, DiscFactory.Create(kind, _aiPlayerId));
                    }
                }
            }

            // 2) fallback to random legal move
            var legal = new List<int>();
            for (int c = 0; c < board.Cols; c++)
            {
                if (board.IsColumnLegal(c)) legal.Add(c);
            }
            if (legal.Count == 0) return null;

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

            int pickedCol = legal[_random.Next(legal.Count)];
            var pickedDisc = DiscFactory.Create(LastChosenDiscKind, _aiPlayerId);
            if (!board.IsDiscLegal(pickedDisc)) return null;
            return new PlaceDiscMove(pickedCol, pickedDisc);
        }

        // no need to compute first empty row with simulation approach
    }
}
