﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineUpSeries
{
    public class Cell
    {
        public int Row { get; }
        public int Col { get; }

        public Disc? Disc { get; set; }
        public int Owner { get; set; }
        public bool IsEmpty => Disc == null;

        public Cell(int row, int col)
        {
            Row = row;
            Col = col;
            Disc = null;
            Owner = 0;
        }
    }
}
