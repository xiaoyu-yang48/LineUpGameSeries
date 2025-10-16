using System;
using System.Collections.Generic;
using System.Data;
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
        public Board Board { get; private set; }
        public Player Player1 { get; private set; }
        public Player Player2 { get; private set; }
        public Player CurrentPlayer { get; private set; }

        public int TurnNumber { get; private set; }

        protected readonly IWinRule WinRule;
        protected readonly IAIStrategy AiSrategy;
        public int WinLen => WinRule.WinLen;

        // Common game state fields
        protected bool _gameOver;
        protected bool _player1Win;
        protected bool _player2Win;

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
                Console.WriteLine("4: Load Game");
                Console.WriteLine("5: Exit");
                Console.WriteLine("Select Game Mode:");
                var pick = Console.ReadLine();
                if (pick == null) return;
                pick = pick.Trim();

                if (pick == "5") return;
                else if (pick == "1") { var classic = new LineUpClassic(); classic.Launch(); }
                else if (pick == "2") { var basic = new LineUpBasic(); basic.Launch(); }
                else if (pick == "3") { var spin = new LineUpSpin(); spin.Launch(); }
                else if (pick == "4")
                {
                    LoadAndLaunchGame();
                }
                else Console.WriteLine("Invalid input. Please enter 1-5.");
            }
        }

        /// Loads a saved game and launches it
        private static void LoadAndLaunchGame()
        {
            Console.Write("Enter filename to load (default: savegame.json): ");
            string? filename = Console.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(filename))
                filename = "savegame.json";

            try
            {
                var saveData = FileManager.LoadGame(filename);
                Console.WriteLine($"Game Type: {saveData.GameName}");
                Console.WriteLine($"Turn: {saveData.CurrentState.TurnNumber}");

                // Create win rule with the saved WinLen
                var winRule = new ConnectWinRule(saveData.WinLen);

                // Create a temporary board to initialize the game (will be overwritten by RestoreFromSaveData)
                var tempBoard = new Board(saveData.CurrentState.Board.Rows, saveData.CurrentState.Board.Cols);
                var tempPlayer = new HumanPlayer(1);
                var aiStrategy = new ImmeWinElseRandom(winRule, 2);

                // Create appropriate game instance based on saved game type with correct WinRule
                Game? game = saveData.GameName switch
                {
                    "LineUpClassic" => new LineUpClassic(tempBoard, tempPlayer, winRule, aiStrategy),
                    "LineUpBasic" => new LineUpBasic(tempBoard, tempPlayer, winRule, aiStrategy),
                    "LineUpSpin" => new LineUpSpin(tempBoard, tempPlayer, winRule, aiStrategy),
                    _ => null
                };

                if (game == null)
                {
                    Console.WriteLine($"Unknown game type: {saveData.GameName}");
                    return;
                }

                // Restore the game state
                game.RestoreFromSaveData(saveData);

                // Start the game loop from the restored state
                game.StartGameLoop();

                Console.WriteLine("Press any key to return to main menu...");
                Console.ReadKey();
            }
            catch (System.IO.FileNotFoundException)
            {
                Console.WriteLine($"Save file not found: {filename}");
                Console.WriteLine("Press any key to return to main menu...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load game: {ex.Message}");
                Console.WriteLine("Press any key to return to main menu...");
                Console.ReadKey();
            }
        }

        //Hook for launching flow
        public virtual void Launch() { }

        //Template Pattern - Main game loop template - reference kehao-liu assignment 1
        public void StartGameLoop()
        {
            while (!EndGame())
            {
                ExecuteGameTurn();
            }

            DisplayGameResult();
        }

        protected virtual void InitializeGameloop() { }
        protected virtual bool EndGame() => true;
        protected virtual void ExecuteGameTurn() { }

        public void SetPlayer1(Player p) => Player1 = p;
        public void SetPlayer2(Player p) => Player2 = p;
        public Player GetPlayerById(int id) => id == 1 ? Player1 : Player2;

        protected void SwitchPlayer()
        {
            CurrentPlayer = CurrentPlayer == Player1 ? Player2 : Player1;
        }

        /// Captures the current game state for undo/redo functionality
        /// Must be called by subclasses before executing a move
        protected GameStateSnapshot CaptureGameState(bool player1Win, bool player2Win, bool gameOver)
        {
            return new GameStateSnapshot(
                Board,
                Player1,
                Player2,
                CurrentPlayer.playerId,
                TurnNumber,
                player1Win,
                player2Win,
                gameOver
            );
        }

        /// Restores a game state from a snapshot  
        /// Restores the full game state (board, players, current turn) from a snapshot.
        /// Safely handles board size mismatches (e.g., after rotation in LineUpSpin).
        protected void RestoreGameState(GameStateSnapshot snapshot)
        {
            if (snapshot == null)
                throw new Exception("RestoreGameState: No snapshot provided.");

            // --- Ensure board matches snapshot dimension
            if (snapshot.BoardClone.Rows != Board.Rows || snapshot.BoardClone.Cols != Board.Cols)
            {
                // Replace the current board with the snapshot clone entirely.
                Board = snapshot.BoardClone.Clone();
            }
            else
            {
                //Restore discs cell-by-cell
                for (int r = 0; r < snapshot.BoardClone.Rows; r++)
                {
                    for (int c = 0; c < snapshot.BoardClone.Cols; c++)
                    {
                        var snapshotDisc = snapshot.BoardClone.Cells[r][c].Disc;

                        if (snapshotDisc != null)
                        {
                            var owner = GetPlayerById(snapshotDisc.PlayerId);
                            Board.Cells[r][c].Disc = DiscFactory.Create(snapshotDisc.Kind, owner);
                        }
                        else
                        {
                            Board.Cells[r][c].Disc = null;
                        }
                    }
                }
            }

            //Restore player inventories
            foreach (var playerId in new[] { 1, 2 })
            {
                if (!snapshot.PlayerInventories.TryGetValue(playerId, out var snapshotInventory))
                    continue;

                var player = GetPlayerById(playerId);

                foreach (DiscKind kind in Enum.GetValues(typeof(DiscKind)))
                {
                    if (snapshotInventory.TryGetValue(kind, out var count))
                        player.Inventory[kind] = count;
                }
            }

            //Restore current player
            CurrentPlayer = snapshot.CurrentPlayerId == 1 ? Player1 : Player2;
        }


        /// Performs an undo operation
        /// Undoes ONE move to go back to the beginning of the previous turn 
        /// Returns true if undo was successful
        protected bool PerformUndo(out bool player1Win, out bool player2Win, out bool gameOver)
        {
            player1Win = false;
            player2Win = false;
            gameOver = false;

            // Need to undo move to go back to beginning of previous turn
            if (MoveManager.GetUndoCount() <= 1)
            {
                Console.WriteLine("Not enough moves to undo.");
                return false;
            }

            // Undo move
            var previousState = MoveManager.Undo();
            if (previousState == null)
            {
                Console.WriteLine("Cannot undo further");
                return false;
            }

            RestoreGameState(previousState);
            player1Win = previousState.Player1Win;
            player2Win = previousState.Player2Win;
            gameOver = previousState.GameOver;
            TurnNumber = previousState.TurnNumber;

            // Restore correct player
            CurrentPlayer = GetPlayerById(previousState.CurrentPlayerId);
            Console.WriteLine($"Undid one move. It's now Player {CurrentPlayer.playerId}'s turn.");
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

            // Need to redo ONE move
            if (MoveManager.GetRedoCount() < 1)
            {
                Console.WriteLine($"Cannot redo");
                return false;
            }
            // Redo the next move
            var nextState = MoveManager.Redo();
            if (nextState == null)
            {
                Console.WriteLine("Cannot redo any further.");
                return false;
            }
            // Restore next state
            RestoreGameState(nextState);
            player1Win = nextState.Player1Win;
            player2Win = nextState.Player2Win;
            gameOver = nextState.GameOver;

            // Restore correct player
            CurrentPlayer = GetPlayerById(nextState.CurrentPlayerId);

            Console.WriteLine($"Redid one move. It's now Player {CurrentPlayer.playerId}'s turn.");
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

        /// Captures the current game state snapshot for saving
        /// Should be overridden by subclasses to provide proper win state
        public virtual GameStateSnapshot CaptureCurrentSnapshot()
        {
            return CaptureGameState(false, false, false);
        }

        /// Gets the undo stack from MoveManager for saving
        public Stack<GameStateSnapshot> GetUndoStack()
        {
            return MoveManager.GetUndoStackForSave();
        }

        /// Restores the game from saved data
        public void RestoreFromSaveData(GameSaveData saveData)
        {
            if (saveData == null)
                throw new ArgumentNullException(nameof(saveData));

            // Restore turnNumber which is important to spin game
            TurnNumber = saveData.TurnNumber;

            // Restore players (they need to be recreated with correct types)
            Player1 = FileManager.ConvertDataToPlayer(saveData.Player1, WinRule);
            Player2 = FileManager.ConvertDataToPlayer(saveData.Player2, WinRule);

            // Restore current game state
            var currentSnapshot = FileManager.ConvertDataToSnapshot(saveData.CurrentState, Player1, Player2);
            RestoreGameState(currentSnapshot);

            // Restore undo/redo stacks in MoveManager
            var undoSnapshots = saveData.UndoStack
                .Select(data => FileManager.ConvertDataToSnapshot(data, Player1, Player2))
                .ToList();

            MoveManager.RestoreStacks(undoSnapshots, saveData.CurrentState.TurnNumber);
            Console.WriteLine($"Game state restored: WinLen: {WinRule.WinLen}");
        }

        protected void PrintHelp()
        {
            Console.WriteLine("==== LineUp Game Help ====");
            Console.WriteLine("\nGame Objective:");
            Console.WriteLine($"  Align {WinLen} discs in a row (horizontally, vertically, or diagonally) to win!");
            Console.WriteLine("\nHow to Play:");
            Console.WriteLine("  Enter your move in format: [Column][DiscType]");
            Console.WriteLine("  Example: 1O places an Ordinary disc in column 1");
            Console.WriteLine("\nDisc Types:");
            Console.WriteLine("  O - Ordinary: Standard disc");
            Console.WriteLine("  B - Boring: Cannot be part of a winning line");
            Console.WriteLine("  M - Magnetic: Pulls adjacent discs when placed");
            Console.WriteLine("  E - Explosive: Clears surrounding discs when placed");
            Console.WriteLine("\nBoard Symbols:");
            Console.WriteLine("  Player 1: @ (Ordinary), B (Boring), M (Magnetic), E (Explosive)");
            Console.WriteLine("  Player 2: # (Ordinary), b (Boring), m (Magnetic), e (Explosive)");
            Console.WriteLine("\nCommands:");
            Console.WriteLine("  Q or q     - Quit game");
            Console.WriteLine("  H or h     - Show this help");
            Console.WriteLine("  Undo       - Undo last turn (2 moves)");
            Console.WriteLine("  Redo       - Redo last undone turn (2 moves)");
            Console.WriteLine("  Save       - Save current game state");
            Console.WriteLine("  Load       - Load saved game state");
            Console.WriteLine("==========================\n");
        }

        /// Common helper method for prompting player mode selection
        protected static bool PromptVsMode()
        {
            while (true)
            {
                Console.WriteLine("1: human vs human");
                Console.WriteLine("2: human vs computer");
                Console.WriteLine("Choose player mode:");

                var mode = Console.ReadLine();
                if (mode == null) return false;
                mode = mode.Trim();
                if (mode == "1") return false;
                if (mode == "2") return true;
                Console.WriteLine("Invalid input");
            }
        }

        /// Common helper method for printing the board
        protected void PrintBoard()
        {
            int rows = Board.Rows;
            int cols = Board.Cols;

            for (int r = rows - 1; r >= 0; r--)
            {
                for (int c = 0; c < cols; c++)
                {
                    char discSymbol;

                    Cell cell = Board.GetCell(r, c);
                    var disc = cell.Disc;
                    if (disc != null)
                    {
                        DiscKind kind = disc.Kind;
                        int owner = disc.PlayerId;

                        discSymbol = (owner, kind) switch
                        {
                            (1, DiscKind.Ordinary) => '@',
                            (1, DiscKind.Boring) => 'B',
                            (1, DiscKind.Magnetic) => 'M',
                            (1, DiscKind.Explosive) => 'E',
                            (2, DiscKind.Ordinary) => '#',
                            (2, DiscKind.Boring) => 'b',
                            (2, DiscKind.Magnetic) => 'm',
                            (2, DiscKind.Explosive) => 'e',
                            _ => ' '
                        };
                    }
                    else
                    {
                        discSymbol = ' ';
                    }

                    Console.Write($"|{discSymbol}");
                }
                Console.WriteLine("|");
            }
            for (int c = 1; c <= cols; c++)
            {
                Console.Write($" {c}");
            }
            Console.WriteLine();
        }

        /// Common method for prompting human move with command handling
        /// Subclasses can override ParseMoveInput to customize input parsing
        protected virtual bool PromptHumanMove(out int col, out DiscKind kind, out string cmd)
        {
            col = -1;
            kind = DiscKind.Ordinary;

            while (true)
            {
                PrintBoard();
                PrintMovePrompt();

                cmd = Console.ReadLine();

                // Handle quit
                if (string.Equals(cmd, "q", StringComparison.OrdinalIgnoreCase))
                    return false;

                // Handle help
                if (string.Equals(cmd, "h", StringComparison.OrdinalIgnoreCase))
                {
                    PrintHelp();
                    continue;
                }

                // Handle undo
                if (string.Equals(cmd, "undo", StringComparison.OrdinalIgnoreCase))
                {
                    if (PerformUndo(out _player1Win, out _player2Win, out _gameOver))
                    {
                        return true; // Exit to let game loop continue with restored player
                    }
                    continue;
                }

                // Handle redo
                if (string.Equals(cmd, "redo", StringComparison.OrdinalIgnoreCase))
                {
                    if (PerformRedo(out _player1Win, out _player2Win, out _gameOver))
                    {
                        return true; // Exit to let game loop continue with restored player
                    }
                    continue;
                }

                // Handle save
                if (string.Equals(cmd, "save", StringComparison.OrdinalIgnoreCase))
                {
                    Console.Write("Enter filename to save (default: savegame.json): ");
                    string? filename = Console.ReadLine()?.Trim();
                    if (string.IsNullOrWhiteSpace(filename))
                        filename = "savegame.json";

                    try
                    {
                        FileManager.SaveGame(filename, this);
                        Console.WriteLine($"Game saved successfully to {filename}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to save game: {ex.Message}");
                    }
                    continue;
                }

                // Handle load
                if (string.Equals(cmd, "load", StringComparison.OrdinalIgnoreCase))
                {
                    Console.Write("Enter filename to load (default: savegame.json): ");
                    string? filename = Console.ReadLine()?.Trim();
                    if (string.IsNullOrWhiteSpace(filename))
                        filename = "savegame.json";

                    try
                    {
                        var saveData = FileManager.LoadGame(filename);
                        RestoreFromSaveData(saveData);
                        Console.WriteLine($"Game loaded successfully from {filename}");
                        return true; // Exit to let game loop continue with restored state
                    }
                    catch (System.IO.FileNotFoundException)
                    {
                        Console.WriteLine($"Save file not found: {filename}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to load game: {ex.Message}");
                    }
                    continue;
                }

                // Parse the move input (delegate to subclass for customization)
                if (ParseMoveInput(cmd, out col, out kind))
                {
                    return true;
                }
            }
        }

        /// Override this in subclasses to customize move input parsing
        /// Returns true if parsing succeeded, false otherwise
        protected virtual bool ParseMoveInput(string? line, out int col, out DiscKind kind)
        {
            col = -1;
            kind = DiscKind.Ordinary;

            if (string.IsNullOrWhiteSpace(line))
            {
                Console.WriteLine("Empty input");
                return false;
            }

            line = line.Trim();

            // Default: expect format like "1B" (column + disc type)
            char last = line[line.Length - 1];
            if (!char.IsLetter(last))
            {
                Console.WriteLine("Wrong format");
                return false;
            }

            string numPart = line.Substring(0, line.Length - 1);
            if (!int.TryParse(numPart, out int colInput))
            {
                Console.WriteLine("Invalid column number");
                return false;
            }

            int promptCol = colInput - 1;
            if (promptCol < 0 || promptCol >= Board.Cols)
            {
                Console.WriteLine("Column number out of range");
                return false;
            }

            char kindch = char.ToUpperInvariant(last);
            kind = kindch switch
            {
                'O' => DiscKind.Ordinary,
                'B' => DiscKind.Boring,
                'M' => DiscKind.Magnetic,
                'E' => DiscKind.Explosive,
                _ => DiscKind.Ordinary
            };

            col = promptCol;
            return true;
        }

        /// Override this to customize the move prompt message
        protected virtual void PrintMovePrompt()
        {
            PrintInventory(CurrentPlayer);
            Console.WriteLine("Enter your move: e.g., 1B for BoringDisc in Column 1");
            Console.WriteLine("Commands: Q: quit, H: help, Undo: undo, Redo: redo, Save: save game, Load: load game");
        }

        /// Override this to customize inventory display
        protected virtual void PrintInventory(Player p)
        {
            Console.WriteLine($"stock P{p.playerId}: Ordinary = {p.Inventory[DiscKind.Ordinary]}, Boring = {p.Inventory[DiscKind.Boring]}, Magnetic = {p.Inventory[DiscKind.Magnetic]}, Explosive = {p.Inventory[DiscKind.Explosive]}");
        }

        /// Common helper method for applying a move
        protected bool ApplyMove(int col, DiscKind kindToUse)
        {
            if (!_player1Win && !_player2Win && Board.IsFull())
            {
                _gameOver = true;
                return false;
            }

            if (!Board.IsColumnLegal(col))
            {
                Console.WriteLine("Column is full or illegal, please try again.");
                return false;
            }

            // Check if player has the disc kind before consuming
            if (!CurrentPlayer.CanUse(kindToUse))
            {
                Console.WriteLine("Insufficient disc type. Please choose another disc kind.");
                return false;
            }

            // Consume the disc first
            if (!CurrentPlayer.TryConsume(kindToUse))
            {
                Console.WriteLine("Failed to consume disc.");
                return false;
            }

            // Create and place the disc
            var disc = DiscFactory.Create(kindToUse, CurrentPlayer);
            var move = new PlaceDiscMove(col, disc);
            move.Execute(Board);
            if (!move.WasPlaced)
            {
                Console.WriteLine("Failed to place a disc. Please retry.");
                // Return the disc back to inventory since placement failed
                CurrentPlayer.AddStock(kindToUse, 1);
                return false;
            }

            //wincheck
            var rule = (WinRule as ConnectWinRule) ?? new ConnectWinRule(WinLen);
            var change = move.ChangeCells;
            if (change == null)
            {
                return true;
            }
            rule.WinCheck(Board, change, out _player1Win, out _player2Win);

            if (_player1Win || _player2Win || Board.IsFull())
            {
                _gameOver = true;
                return true;
            }

            TurnNumber++;

            SwitchPlayer();
            // Save state AFTER executing move and switching player
            var snapshot = CaptureGameState(_player1Win, _player2Win, _gameOver);
            MoveManager.SaveState(snapshot);
            return true;
        }
        protected virtual void DisplayGameResult()
        {
            PrintBoard();
            Console.WriteLine("Game Over");
            if (_player1Win || _player2Win) WinResult(_player1Win, _player2Win);
            else if (_player1Win == true && _player2Win == true) Console.WriteLine("Draw End");
        }
    }
}
