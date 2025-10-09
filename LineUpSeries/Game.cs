using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineUpSeries
{
    // template pattern
    public abstract class Game
    {
        public abstract string Name { get; }
        private const string SaveDirectory = "SavedGames";
        private const string DefaultSaveFile = "SavedGame.json";
        public static void Start()
        { 
        }
    }
}
