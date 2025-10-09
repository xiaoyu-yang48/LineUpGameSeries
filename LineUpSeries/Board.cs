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
            Cells = new Cell[rows][];
            for (int r = 0; r < rows; r++)
            {
                Cells[r] = new Cell[cols];
                for (int c = 0; c < cols; c++)
                {
                    Cells[r][c] = new Cell(r, c);
                }
            }
        }

        public bool InBounds(int row, int col) => row >= 0 && row < Rows && col >= 0 && col < Cols;

        public Cell GetCell(int row, int col)
        {
            if (!InBounds(row, col)) throw new ArgumentOutOfRangeException();
            return Cells[row][col];
        }

        public bool IsColumnLegal(int col)
        {
            if (col < 0 || col >= Cols) return false;
            return GetCell(Rows - 1, col).IsEmpty;
        }

        public bool IsDiscLegal(Disc disc)
        {
            if (disc == null) return false;
            var player = disc.PlayerId == 1 ? Player.Player1 : Player.Player2;
            return player.CanUse(disc.Kind);
        }

        //place a disc into a column
        public int PlaceDisc(int col, Disc disc)
        {
            if (!IsColumnLegal(col) || !IsDiscLegal(disc)) return -1;
            for (int r = 0; r < Rows; r++)
            {
                if (Cells[r][col].Disc == null)
                {
                    Cells[r][col].Disc = disc;
                    return r;
                }
            }
            return -1;
        }

        //apply gravity to whole board
        public void ApplyGravity()
        {
            for (int c = 0; c < Cols; c++)
            {
                int nextFillRow = 0;
                for (int r = 0; r < Rows; r++)
                {
                    var disc = Cells[r][c].Disc;
                    if (disc != null)
                    {
                        if (nextFillRow != r)
                        {
                            Cells[nextFillRow][c].Disc = disc;
                            Cells[r][c].Disc = null;
                        }
                        nextFillRow++;
                    }
                }
            }
        }

        public bool IsFull()
        {
            for (int c = 0; c < Cols; c++)
            {
                if (IsColumnLegal(c)) return false;
            }
            return true;
        }

        public Board Clone()
        {
            var clone = new Board(Rows, Cols);
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    var disc = Cells[r][c].Disc;
                    if (disc != null)
                    {
                        clone.Cells[r][c].Disc = DiscFactory.Create(disc.Kind, disc.PlayerId);
                    }
                }
            }
            return clone;
        }

        //rotate clockwise
        public void RotateCW()
        {
        }
    }
}
