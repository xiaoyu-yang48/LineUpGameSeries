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
        public int DiscOwner { get; }

        // hook for special disc behavior
        public virtual void OnPlaced(Board board, int row, int col) { }
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
    }

    public sealed class BoringDisc : Disc
    {
        public override DiscKind Kind => DiscKind.Boring;
    }

    public sealed class MagneticDisc : Disc
    {
        public override DiscKind Kind => DiscKind.Magnetic;
    }

    public sealed class ExplosiveDisc : Disc
    {
        public override DiscKind Kind => DiscKind.Explosive;
    }
}
