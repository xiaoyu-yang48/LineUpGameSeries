using System;
using System.Collections.Generic;


namespace LineUpSeries
{
    public interface IAIStrategy
    {
        PlaceDiscMove? PickMove(Board board, Player aiPlayer);
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

        public PlaceDiscMove? PickMove(Board board, Player aiPlayer)
        {

            for (int c = 0; c < board.Cols; c++)
            {
                if (!board.IsColumnLegal(c)) continue;
                foreach (DiscKind kind in Enum.GetValues(typeof(DiscKind)))
                {
                    if (!aiPlayer.CanUse(kind)) continue;
                    var disc = DiscFactory.Create(kind, aiPlayer);
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
                        return new PlaceDiscMove(c, DiscFactory.Create(kind, aiPlayer));
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
                    if (aiPlayer.CanUse(kind))
                    {
                        PickedDiscKind = kind;
                        break;
                    }
                }
            }
            int pickedCol = playableCol[_random.Next(playableCol.Count)];
            if (!aiPlayer.CanUse(PickedDiscKind)) return null;
            var pickedDisc = DiscFactory.Create(PickedDiscKind, aiPlayer);
            return new PlaceDiscMove(pickedCol, pickedDisc);
        }
    }
}
