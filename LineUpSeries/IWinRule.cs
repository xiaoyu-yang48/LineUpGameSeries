using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace LineUpSeries
{
    public interface IWinRule
    {
        int WinLen { get; }
        bool CheckCellWin(Board board, Cell cell);
    }

    public class WinRule : IWinRule
    {
        public int WinLen { get; }

        public WinRule(int winLen)
        {
            if (winLen <= 1) throw new ArgumentException("WinLen is too short");
            WinLen = winLen;
        }

        public bool CheckCellWin(Board board, Cell cell)
        {
            int row = cell.Row;
            int col = cell.Col;

            if (cell.Disc == null) return false;
            int playerId = cell.Disc.PlayerId;


            // wincheck logic
            var directions = new (int dr, int dc)[] 
            {
                (0, 1),  // horizontal
                (1, 0),  // vertical
                (1, 1),  // diagonal 
                (1, -1)  // diagonal 
            };

            // check direction
            foreach (var (dr, dc) in directions)
            {
                int count = 1;

                for (int i = 1; i < WinLen; i++)
                {
                    int r = row + i * dr;
                    int c = col + i * dc;
                    // boundary check
                    if (!board.InBounds(r, c)) break;
                    var other = board.Cells[r][c];
                    if (other?.Disc?.PlayerId == playerId) 
                    {
                        count++;
                    }
                    else break;
                }

                for (int i = 1; i < WinLen; i++)
                {
                    int r = row - i * dr;
                    int c = col - i * dc;
                    // boundary check
                    if (!board.InBounds(r, c)) break;
                    var other = board.Cells[r][c];
                    if (other?.Disc?.PlayerId == playerId)
                    {
                        count++;
                    }
                    else break;
                }

                if (count >= WinLen) return true;
            }
            return false;
        }

        public void WinCheck(Board board, ChangeCell changeCells, out bool player1Win, out bool player2Win)
        {
            player1Win = false;
            player2Win = false;

            if (changeCells == null) return;

            foreach (var cell in changeCells.Cells)
            {
                int r = cell.Row;
                int c = cell.Col;

                if (!board.InBounds(r, c)) continue;

                int playerId = cell.Disc?.PlayerId ?? 0;
                if (playerId == 0) continue;
                if (CheckCellWin(board, cell))
                {
                    if (playerId == 1) player1Win = true;
                    else if (playerId == 2) player2Win = true;

                    if (player1Win && player2Win) return;
                }
            }
        }
    }
}
