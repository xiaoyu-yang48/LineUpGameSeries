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
        private Player(int id) { PlayerId = id; }
        public static Player Player1 { get; } = new Player(1);
        public static Player Player2 { get; } = new Player(2);
    }
}
