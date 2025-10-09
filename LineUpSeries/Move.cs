using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineUpSeries
{
    public abstract class Move
    {
        public ChangeCell ChangeCells = new ChangeCell();

        public abstract void Execute(Board board);
        public abstract void Unexecute(Board board);
    }

    public class PlaceDiscMove : Move 
    {
        private int Col { get; }
        private Disc Disc { get; }
        private int RowPlaced { get; set; } = -1;
        public bool WasPlaced => RowPlaced >= 0;

        public PlaceDiscMove(int col, Disc disc)
        {
            Col = col;
            Disc = disc;
        }

        public override void Execute(Board board)
        {
            RowPlaced = board.PlaceDisc(Col, Disc);
            if (RowPlaced < 0) return;

            ChangeCells = new ChangeCell();
            Disc.OnPlaced(board, RowPlaced, Col, ChangeCells);
        }

        public override void Unexecute(Board board)
        {
            if (RowPlaced < 0) return;
            // Remove placed disc and re-apply gravity to restore state approximately
            board.Cells[RowPlaced][Col].Disc = null;
            board.ApplyGravity();
        }
    }
}
