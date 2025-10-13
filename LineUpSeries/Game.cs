using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LineUpSeries
{
    // template pattern
    public abstract class Game
    {
        public abstract string Name { get; }
        public Board Board { get; }
        public Player CurrentPlayer { get; private set; } = Player.Player1;

        protected readonly IWinRule WinRule;
        protected readonly IAIStrategy AiSrategy;
        public int WinLen => WinRule.WinLen;

        // Move history management - using Singleton pattern
        protected MoveManager MoveManager => MoveManager.Instance;

        protected Game (Board board, Player currentPlayer, IWinRule winRule, IAIStrategy aiSrategy)
        {
            Board = board;
            CurrentPlayer = currentPlayer;
            WinRule = winRule;
            AiSrategy = aiSrategy;
            MoveManager.Clear();
        }

        //default constructor
        protected Game() : this(new Board(8, 9), new HumanPlayer(1), new ConnectWinRule(4), new ImmeWinElseRandom(new ConnectWinRule(4)))
        { }

        //Template Pattern - Game launcher to provide menu and setup
        public static void Run() 
        {
            while (true)
            {
                Console.WriteLine("==== LineUp Series ====");
                Console.WriteLine("1: Classic");
                Console.WriteLine("2: Basic");
                Console.WriteLine("3: Spin");
                Console.WriteLine("4: Exit");
                Console.WriteLine("Select Game Mode:");
                var pick = Console.ReadLine();
                if (pick == null) return;
                pick = pick.Trim();

                if (pick == "4") return;
                else if (pick == "1") { var classic = new LineUpClassic(); classic.Launch(); }
                else if (pick == "2") { var basic = new LineUpBasic(); basic.Launch(); }
                else if (pick == "3") { var spin = new LineUpSpin(); spin.Launch(); }

                else Console.WriteLine("Invalid input. Please enter 1-4.");
            }
        }

        //Hook for launching flow
        public virtual void Launch() { }

        //Template Pattern - Main game loop template - reference kehao-liu assignment 1
        public void StartGameLoop()
        {
            InitializeGameloop();

            while (!EndGame())
            {
                ExecuteGameTurn();
            }

            DisplayGameResult();
        }

        protected virtual void InitializeGameloop() { }
        protected virtual bool EndGame() => true;
        protected virtual void ExecuteGameTurn() { }
        protected virtual void DisplayGameResult() { }

        protected void SwitchPlayer()
        {
            CurrentPlayer = CurrentPlayer == Player.Player1 ? Player.Player2 : Player.Player1;
        }

        /// Captures the current game state for undo/redo functionality
        /// Must be called by subclasses before executing a move
        protected GameStateSnapshot CaptureGameState(bool player1Win, bool player2Win, bool gameOver)
        {
            return new GameStateSnapshot(
                Board,
                Player.Player1,
                Player.Player2,
                CurrentPlayer.playerId,
                MoveManager.CurrentTurn,
                player1Win,
                player2Win,
                gameOver
            );
        }

        /// Restores a game state from a snapshot
        /// Must be implemented by subclasses to restore game-specific state
        protected void RestoreGameState(GameStateSnapshot snapshot)
        {
            if (snapshot == null)
            {
                throw new Exception("something wrong: there are no snapshots detected.");
            }

            // Restore board state
            for (int r = 0; r < Board.Rows; r++)
            {
                for (int c = 0; c < Board.Cols; c++)
                {
                    var snapshotDisc = snapshot.BoardClone.Cells[r][c].Disc;
                    if (snapshotDisc != null)
                    {
                        Board.Cells[r][c].Disc = DiscFactory.Create(snapshotDisc.Kind, snapshotDisc.PlayerId);
                    }
                    else
                    {
                        Board.Cells[r][c].Disc = null;
                    }
                }
            }

            // Restore player inventories
            foreach (var playerId in new[] { 1, 2 })
            {
                var player = Player.GetById(playerId);
                var snapshotInventory = snapshot.PlayerInventories[playerId];

                foreach (DiscKind kind in Enum.GetValues(typeof(DiscKind)))
                {
                    player.Inventory[kind] = snapshotInventory[kind];
                }
            }

            // Restore current player
            CurrentPlayer = snapshot.CurrentPlayerId == 1 ? Player.Player1 : Player.Player2;
        }

        /// Performs an undo operation, returning the game state and win flags
        /// Undoes TWO moves to go back to the beginning of the previous turn (tricky)
        /// Returns true if undo was successful
        protected bool PerformUndo(out bool player1Win, out bool player2Win, out bool gameOver)
        {
            player1Win = false;
            player2Win = false;
            gameOver = false;

            // Need to undo 2 moves to go back to beginning of previous turn
            if (MoveManager.GetUndoCount() < 2)
            {
                Console.WriteLine("Not enough moves to undo.");
                return false;
            }

            // Undo first move
            var firstState = MoveManager.Undo();
            if (firstState == null)
            {
                Console.WriteLine("Cannot undo to before the game started.");
                return false;
            }

            // Undo second move
            var secondState = MoveManager.Undo();
            if (secondState == null)
            {
                Console.WriteLine("Cannot undo to before the game started.");
                return false;
            }

            RestoreGameState(secondState);
            player1Win = secondState.Player1Win;
            player2Win = secondState.Player2Win;
            gameOver = secondState.GameOver;

            Console.WriteLine($"Undone to turn {MoveManager.CurrentTurn}.");
            return true;
        }

        /// Performs a redo operation by N turns, returning the game state and win flags
        /// Redoes TWO moves to go forward to the beginning of the next turn (tricky)
        /// Returns true if redo was successful
        protected bool PerformRedo(out bool player1Win, out bool player2Win, out bool gameOver)
        {
            player1Win = false;
            player2Win = false;
            gameOver = false;

            // Need to redo 2 moves to go forward to beginning of next turn
            if (MoveManager.GetRedoCount() < 2)
            {
                Console.WriteLine($"Cannot redo. Only {MoveManager.GetRedoCount()} moves available.");
                return false;
            }

            // Redo first move
            _ = MoveManager.Redo();
            // Redo second move
            var secondState = MoveManager.Redo();

            RestoreGameState(secondState);
            player1Win = secondState.Player1Win;
            player2Win = secondState.Player2Win;
            gameOver = secondState.GameOver;
            return true;
        }

        public void WinResult(bool player1Win, bool player2Win)
        {
            if (player1Win && !player2Win)
            {
                Console.WriteLine($"Player 1 wins!");
            }
            //check if current player's move leads to opponent winning
            else if (player2Win && !player1Win)
            {
                Console.WriteLine($"Player 2 wins!");
            }
            else if (player1Win && player2Win)
            {
                Console.WriteLine($"Players 1 and 2 both aligned this turn. It's a draw!");
            }
        }
    }
}
