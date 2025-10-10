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

    public sealed class ImmeWinElseRandom : IAIStrategy
    {
        private readonly IWinRule _winRule;
        private readonly int _aiPlayerId;
        private readonly Random _random = new Random();
        public DiscKind PickedDiscKind { get; private set; } = DiscKind.Ordinary;

        public ImmeWinElseRandom(IWinRule winRule, int aiPlayerId = 2)
        { 
            _winRule = winRule;
            _aiPlayerId = aiPlayerId;
        }

        public PlaceDiscMove? PickMove(Board board)
        { 
            var aiPlayer = _aiPlayerId == 2 ? Player.Player2 : Player.Player1;

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
                    bool p1w = false;
                    bool p2w = false;
                    var rule = (_winRule as ConnectWinRule) ?? new ConnectWinRule(_winRule.WinLen);
                    rule.WinCheck(simBoard, move.ChangeCells, out p1w, out p2w);
                    bool win = (_aiPlayerId == 2 ? p2w  : p1w);
                    if (win)
                    {
                        PickedDiscKind = kind;
                        return new PlaceDiscMove(c, DiscFactory.Create(kind, _aiPlayerId));
                    }
                }
            }

            var playableCol = new List<int>();
            for (int c = 0; c < board.Cols; c++)
            {
                if (board.IsColumnLegal(c)) playableCol.Add(c);
            }
            if (playableCol.Count == 0) return null;

            //prefer ordinary, else randomly choose disc
            if (aiPlayer.Inventory.TryGetValue(DiscKind.Ordinary, out var ordinaryCount) && ordinaryCount > 0)
            {
                PickedDiscKind = DiscKind.Ordinary;
            }
            else 
            {
                foreach (DiscKind kind in Enum.GetValues(typeof(DiscKind)))
                {
                    var disc = DiscFactory.Create(kind, _aiPlayerId);
                    if (board.IsDiscLegal(disc))
                    {
                        PickedDiscKind = kind;
                        break;
                    }
                }
            }
            int pickedCol = playableCol[_random.Next(playableCol.Count)];
            var pickedDisc = DiscFactory.Create(PickedDiscKind, _aiPlayerId);
            if (!board.IsDiscLegal(pickedDisc)) return null;
            return new PlaceDiscMove(pickedCol, pickedDisc);
        }
    }
}
