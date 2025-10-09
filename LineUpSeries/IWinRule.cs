using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineUpSeries
{
    public class IWinRule
    {
        public int WinLen { get; }
        public IWinRule(int winLen) { WinLen = winLen; }

        public bool CheckCellWin(Board board, Cell cell)
        {
            int rows = board.Rows;
            int cols = board.Cols;
            int row = cell.Row;
            int col = cell.Col;

            int player = 
        }
    }
}
