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

        public LineUpBasic(Board board, Player currentPlayer, IWinRule winRule, IAIStrategy aiSrategy) : base(board, currentPlayer, winRule, aiSrategy)
        {
        }


        public LineUpBasic() : base() { }

        public override void Launch()
        {
            Console.WriteLine("==== LineUpBasic ====");
        }
    }
}
