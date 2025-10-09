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
            if (InBounds(row, col)) throw new ArgumentOutOfRangeException();
            return Cells[row][col];
        }

        public bool IsLegalMove(int col)
        {
            if (col < 0 || col >= Cols) return false;
            return GetCell(Rows - 1, col).IsEmpty;
        }

        //place a disc into a column
        public int PlaceDisc(int col, Disc disc)
        {
            if (!IsLegalMove(col)) return -1;
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
            for (int c = 0; c <= Cols; c++)
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
                    }
                }
            }
        }

        //rotate clockwise
        public void RotateCW()
        {
        }
    }
}
