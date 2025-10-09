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
        public OrdinaryDisc(int playerId) : base(playerId) {}
    }

    public sealed class BoringDisc : Disc
    {
        public override DiscKind Kind => DiscKind.Boring;
        public BoringDisc(int playerId) : base(playerId) {}
    }

    public sealed class MagneticDisc : Disc
    {
        public override DiscKind Kind => DiscKind.Magnetic;
        public MagneticDisc(int playerId) : base(playerId) {}
    }

    public sealed class ExplosiveDisc : Disc
    {
        public override DiscKind Kind => DiscKind.Explosive;
        public ExplosiveDisc(int playerId) : base(playerId) {}
        public override void OnPlaced(Board board, int row, int col, ChangeCell changeCells)
        {
            for (int r = row - 1; r <= row + 1; r++)
            {
                for (int c = col - 1; c <= col + 1; c++)
                {
                    if (board.InBounds(r, c))
                    {
                        board.Cells[r][c].Disc = null;
                        changeCells.Add(board.Cells[r][c]);
                    }
                }
            }
        }
    }
}
