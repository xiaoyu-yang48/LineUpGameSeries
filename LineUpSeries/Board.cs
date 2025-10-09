using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineUpSeries
{
    public class Board
    {
        public int Rows { get; }
        public int Cols { get; }
        public Cell[][] Cells { get; }

        public Board(int rows, int cols) 
        { 
            Rows = rows;
            Cols = cols;
        }
    }
}
