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

        // Variant-specific default stock profiles
        private static readonly Dictionary<GameVariant, Dictionary<DiscKind, int>> VariantStock = new()
        {
            {
                GameVariant.Classic,
                new Dictionary<DiscKind, int>
                {
                    { DiscKind.Ordinary, 42 },
                    { DiscKind.Boring, 0 },
                    { DiscKind.Magnetic, 0 },
                    { DiscKind.Explosive, 0 },
                }
            },
            {
                GameVariant.Basic,
                new Dictionary<DiscKind, int>
                {
                    { DiscKind.Ordinary, 42 },
                    { DiscKind.Boring, 0 },
                    { DiscKind.Magnetic, 0 },
                    { DiscKind.Explosive, 0 },
                }
            },
            {
                GameVariant.Spin,
                new Dictionary<DiscKind, int>
                {
                    { DiscKind.Ordinary, 42 },
                    { DiscKind.Boring, 0 },
                    { DiscKind.Magnetic, 0 },
                    { DiscKind.Explosive, 0 },
                }
            }
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
            return GetDefaultStock(kind, GameVariant.Classic);
        }

        public static int GetDefaultStock(DiscKind kind, GameVariant variant)
        {
            if (VariantStock.TryGetValue(variant, out var stock) && stock.TryGetValue(kind, out var count))
            {
                return count;
            }
            return 0;
        }

        public static void ApplyProfile(Player player, GameVariant variant)
        {
            foreach (var k in GetAllKinds())
            {
                player.Inventory[k] = GetDefaultStock(k, variant);
            }
        }

        public static string KindListString()
        {
            return string.Join("|", Enum.GetNames(typeof(DiscKind)).Select(n => n.ToLowerInvariant()));
        }
    }
}
