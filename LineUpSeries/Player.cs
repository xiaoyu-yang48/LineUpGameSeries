using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LineUpSeries
{
    public abstract class Player
    {
        public int playerId { get; }
        public Dictionary<DiscKind, int> Inventory { get; }
        public virtual bool IsComputer => false;

        protected Player(int id)
        {
            playerId = id;
            Inventory = new Dictionary<DiscKind, int>();
            foreach (DiscKind kind in Enum.GetValues(typeof(DiscKind)))
            {
                Inventory[kind] = 0;
            }
        }

        public bool CanUse(DiscKind kind)
        { 
            return Inventory.TryGetValue(kind, out var count) && count > 0;
        }
        public bool TryConsume(DiscKind kind)
        {
            if(!Inventory.TryGetValue(kind, out var count) || count <= 0) return false;
            Inventory[kind] = count - 1;
            return true;
        }

        public void AddStock(DiscKind kind, int amount)
        {
            if (!Inventory.ContainsKey(kind)) Inventory[kind] = 0;
            Inventory[kind] += amount;
        }

        public void ReturnDisc(int count = 1)
        {
            Inventory[DiscKind.Ordinary] += count;
        }

        public Player Clone()
        {
            Player clone = this is ComputerPlayer cp
                ? new ComputerPlayer(cp.Strategy, playerId)
                : new HumanPlayer(playerId);

            foreach (var kvp in Inventory)
            {
                clone.Inventory[kvp.Key] = kvp.Value;
            }
            return clone;
        }
    }

    public sealed class HumanPlayer : Player
    {
        public override bool IsComputer => false;
        public HumanPlayer(int id) : base(id) { }
    }

    public sealed class ComputerPlayer : Player
    {
        public override bool IsComputer => true;
        public IAIStrategy Strategy { get; }
        public ComputerPlayer(IAIStrategy strategy, int id = 2) : base(id)
        {
            Strategy = strategy;
        }
    }
}
