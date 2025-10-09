using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineUpSeries
{
    public sealed class LineUpBasic : Game
    {
        public override string Name => "LineUpBasic";
        public LineUpBasic(Board board, Player currentPlayer, IWinRule winRule, IAIStrategy aiStrategy)
            : base(board, currentPlayer, winRule, aiStrategy) {}

        protected override void InitializeGameLoop()
        {
            DiscRegistry.ApplyProfile(Player.Player1, GameVariant.Basic);
            DiscRegistry.ApplyProfile(Player.Player2, GameVariant.Basic);
        }
    }
}
