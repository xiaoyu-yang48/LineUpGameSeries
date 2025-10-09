using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineUpSeries
{
    public class Player
    {
        public int PlayerId { get; }
        public Dictionary<DiscKind, int> Inventory { get; }

        private Player(int id)
        {
            PlayerId = id;
            Inventory = new Dictionary<DiscKind, int>();
            foreach (var k in DiscRegistry.GetAllKinds())
            {
                Inventory[k] = DiscRegistry.GetDefaultStock(k);
            }
        }

        public static Player Player1 { get; } = new Player(1);
        public static Player Player2 { get; } = new Player(2);

        public bool CanUse(DiscKind kind)
        {
            return Inventory.TryGetValue(kind, out var count) && count > 0;
        }

        public bool TryConsume(DiscKind kind)
        {
            if (!Inventory.TryGetValue(kind, out var count) || count <= 0) return false;
            Inventory[kind] = count - 1;
            return true;
        }
        
        public void AddStock(DiscKind kind, int amount)
        {
            if (!Inventory.ContainsKey(kind)) Inventory[kind] = 0;
            Inventory[kind] += amount;
        }
    }
}
