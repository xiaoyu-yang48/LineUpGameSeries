using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineUpSeries
{
    public abstract class Move
    {
        public List<Cell> ChangeCells = new List<Cell>();

        public abstract void Execute(Board board);
        public abstract void Unexecute(Board board);
    }

    public class PlaceDiscMove : Move 
    {
        private int Col { get; }
        private Disc Disc { get; }
        private int RowPlaced { get; set; } = -1;

        public PlaceDiscMove(int col, Disc disc)
        {
            Col = col;
            Disc = disc;
        }

        public override void Execute(Board board)
        {
            RowPlaced = board.PlaceDisc(Col, Disc);
            if (RowPlaced < 0) return;

            ChangeCells.Clear();
            Disc.OnPlaced(board, RowPlaced, Col, ChangeCells);
        }
    }
}
