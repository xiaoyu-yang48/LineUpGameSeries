using System;
using System.Collections.Generic;
using System.Linq;

namespace LineUpSeries
{
    public static class DiscRegistry
    {
        private static readonly Dictionary<DiscKind, Func<int, Disc>> Factories = new()
        {
            { DiscKind.Ordinary, playerId => new OrdinaryDisc(playerId) },
            { DiscKind.Boring,   playerId => new BoringDisc(playerId) },
            { DiscKind.Magnetic, playerId => new MagneticDisc(playerId) },
            { DiscKind.Explosive,playerId => new ExplosiveDisc(playerId) },
        };

        private static readonly Dictionary<DiscKind, int> DefaultStock = new()
        {
            { DiscKind.Ordinary, 42 },
            { DiscKind.Boring, 0 },
            { DiscKind.Magnetic, 0 },
            { DiscKind.Explosive, 0 },
        };

        public static Disc CreateDisc(DiscKind kind, int playerId)
        {
            return Factories.TryGetValue(kind, out var factory)
                ? factory(playerId)
                : new OrdinaryDisc(playerId);
        }

        public static IEnumerable<DiscKind> GetAllKinds()
        {
            return Enum.GetValues(typeof(DiscKind)).Cast<DiscKind>();
        }

        public static int GetDefaultStock(DiscKind kind)
        {
            return DefaultStock.TryGetValue(kind, out var count) ? count : 0;
        }

        public static string KindListString()
        {
            return string.Join("|", Enum.GetNames(typeof(DiscKind)).Select(n => n.ToLowerInvariant()));
        }
    }
}
