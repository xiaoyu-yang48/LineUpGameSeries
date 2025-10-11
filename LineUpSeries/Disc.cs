using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineUpSeries
{
    // disc hierarchy, factory pattern
    public abstract class Disc
    {
        public abstract DiscKind Kind { get; }

        //owner id
        public int PlayerId { get; }
        protected Disc(int playerId)
        {
            PlayerId = playerId;
        }

        // hook for special disc behavior
        public virtual void OnPlaced(Board board, int row, int col, ChangeCell changeCells) 
        {
            var cell = board.Cells[row][col];
            if (cell.Disc != null)
                changeCells.Add(cell);
        }
    }

    public enum DiscKind
    { 
        Ordinary,
        Boring,
        Magnetic,
        Explosive
    }

    public sealed class OrdinaryDisc : Disc 
    {
        public override DiscKind Kind => DiscKind.Ordinary;
        public OrdinaryDisc(int playerId): base(playerId) { }
    }

    public sealed class BoringDisc : Disc
    {
        public override DiscKind Kind => DiscKind.Boring;
        public BoringDisc(int playerId) : base(playerId) { }

        public override void OnPlaced(Board board, int row, int col, ChangeCell changeCells)
        {
            var placedCell = board.Cells[row][col];
            if (placedCell != null && placedCell.Disc is BoringDisc)
            {
                for (int r = 0; r < row; r++)
                {
                    var cell = board.Cells[r][col];
                    if (cell != null && cell.Disc != null)
                    {
                        int owner = cell.Disc.PlayerId;
                        Player p = Player.GetById(owner);
                        if (p != null)
                        {
                            p.ReturnDisc(1);
                        }
                        cell.Disc = null;
                    }
                }
                int placedOwner = placedCell.Disc.PlayerId;
                placedCell.Disc = new OrdinaryDisc(placedOwner);
                board.ApplyGravity();
                Cell changed = board.Cells[0][col];
                changeCells.Add(changed);
            }
        }
    }

    public sealed class MagneticDisc : Disc
    {
        public override DiscKind Kind => DiscKind.Magnetic;
        public MagneticDisc(int playerId) : base(playerId) { }
        public override void OnPlaced(Board board, int row, int col, ChangeCell changeCells)
        {
            var placedCell = board.Cells[row][col];
            if (placedCell != null && placedCell.Disc is MagneticDisc)
            {
                int placedOwner = placedCell.Disc.PlayerId;
                //row == 0, no place underneath
                if (row == 0 || (row > 0 && board.Cells[row - 1][col].Owner == placedOwner))
                {
                    placedCell.Disc = new OrdinaryDisc(placedOwner);
                    changeCells.Add(placedCell);
                    return;
                }
                for (int r = row - 2; r >= 0; r--)
                {
                    var cellToUp = board.Cells[r][col];
                    if (cellToUp != null && cellToUp.Owner == placedOwner)
                    {
                        var cellToDown = board.Cells[r + 1][col];
                        var cellToUpDisc = cellToUp.Disc;
                        cellToUp.Disc = cellToDown.Disc;
                        cellToDown.Disc = cellToUpDisc;

                        placedCell.Disc = new OrdinaryDisc(placedOwner);
                        changeCells.Add(placedCell);
                        changeCells.Add(cellToDown);
                        changeCells.Add((cellToUp));
                        break;
                    }
                }
            }   
        }
    }

    public sealed class ExplosiveDisc : Disc
    {
        public override DiscKind Kind => DiscKind.Explosive;
        public ExplosiveDisc(int playerId) : base(playerId) { }
        public override void OnPlaced(Board board, int row, int col, ChangeCell changeCells)
        {
            for (int r = row - 1; r <= row + 1; r++)
            {
                for (int c = col - 1; c <= col + 1; c++)
                {
                    if (board.InBounds(r, c))
                    {
                        board.Cells[r][c].Disc = null;
                    }
                }
            }
            board.ApplyGravity();
        }
    }

    public static class DiscFactory
    {
        public static Disc Create(DiscKind kind, int playerId)
        {
            return kind switch
            {
                DiscKind.Boring => new BoringDisc(playerId),
                DiscKind.Magnetic => new MagneticDisc(playerId),
                DiscKind.Explosive => new ExplosiveDisc(playerId),
                _ => new OrdinaryDisc(playerId)
            };
        }
    }
}
