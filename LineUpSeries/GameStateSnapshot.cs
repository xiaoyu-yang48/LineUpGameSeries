using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace LineUpSeries
{
    public class GameStateSnapshot
    {
        public Board BoardClone { get; }
        public Dictionary<int, Dictionary<DiscKind, int>> PlayerInventories { get; }
        public int CurrentPlayerId { get; }
        public int TurnNumber { get; }
        public bool Player1Win { get; }
        public bool Player2Win { get; }
        public bool GameOver { get; }

        public GameStateSnapshot(
            Board board,
            Player player1,
            Player player2,
            int currentPlayerId,
            int turnNumber,
            bool player1Win,
            bool player2Win,
            bool gameOver)
        {
            // Deep clone the board
            BoardClone = board.Clone();

            // Clone player inventories
            PlayerInventories = new Dictionary<int, Dictionary<DiscKind, int>>
            {
                { 1, new Dictionary<DiscKind, int>(player1.Inventory) },
                { 2, new Dictionary<DiscKind, int>(player2.Inventory) }
            };

            CurrentPlayerId = currentPlayerId;
            TurnNumber = turnNumber;
            Player1Win = player1Win;
            Player2Win = player2Win;
            GameOver = gameOver;
        }
    }
}
