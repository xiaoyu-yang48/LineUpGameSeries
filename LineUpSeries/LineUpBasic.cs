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
            var stock = new Dictionary<DiscKind, int>
            {
                { DiscKind.Ordinary, 42 },
                { DiscKind.Boring, 0 },
                { DiscKind.Magnetic, 0 },
                { DiscKind.Explosive, 0 },
            };
            Player.Player1.SetInventory(stock);
            Player.Player2.SetInventory(stock);
        }
    }
}
