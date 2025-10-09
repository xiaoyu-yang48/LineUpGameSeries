using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineUpSeries
{
    public abstract class Player
    {
        public int PlayerId { get; }
        public Dictionary<DiscKind, int> Inventory { get; }
        public virtual bool IsComputer => false;

        protected Player(int id)
        {
            PlayerId = id;
            Inventory = new Dictionary<DiscKind, int>();
            foreach (DiscKind k in Enum.GetValues(typeof(DiscKind)))
            {
                Inventory[k] = 0;
            }
        }

        public static Player? Player1 { get; private set; }
        public static Player? Player2 { get; private set; }

        public static void SetPlayer1(Player p) => Player1 = p;
        public static void SetPlayer2(Player p) => Player2 = p;

        public static Player? GetById(int id) => id == 1 ? Player1 : Player2;

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

        public void SetInventory(IDictionary<DiscKind, int> stock)
        {
            foreach (DiscKind k in Enum.GetValues(typeof(DiscKind)))
            {
                Inventory[k] = 0;
            }
            foreach (var kv in stock)
            {
                Inventory[kv.Key] = kv.Value;
            }
        }
    }

    public sealed class HumanPlayer : Player
    {
        public override bool IsComputer => false;
        public HumanPlayer(int id) : base(id) {}
    }

    public sealed class ComputerPlayer : Player
    {
        public override bool IsComputer => true;
        public IAIStrategy Strategy { get; }
        public ComputerPlayer(int id, IAIStrategy strategy) : base(id)
        {
            Strategy = strategy;
        }
    }
}
