using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineUpSeries
{
    public sealed class LineUpBasic : Game
    {
        public override string Name => "LineUpBasic";
        private bool _gameOver;
        private bool _player1Win;
        private bool _player2Win;

        // Fixed grid size for LineUpBasic
        private const int FixedRows = 8;
        private const int FixedCols = 9;
        private const int WinLength = 4;

        public LineUpBasic(Board board, Player currentPlayer, IWinRule winRule, IAIStrategy aiStrategy) : base(board, currentPlayer, winRule, aiStrategy)
        {
        }

        // Default constructor
        public LineUpBasic() : base()
        {
        }

        public override void Launch()
        {
            while (true)
            {
                Console.WriteLine("==== LineUpBasic ====");
                Console.WriteLine("1: New Game");
                Console.WriteLine("2: Back");
                Console.WriteLine("Select: ");

                var sel = Console.ReadLine();
                if (sel == null) return;
                sel = sel.Trim();

                if (sel == "2") return;
                else if (sel == "1")
                {
                    bool isVsComputer = PromptVsMode();

                    var board = new Board(FixedRows, FixedCols);
                    var rule = new ConnectWinRule(WinLength);
                    //rule.SetWinLen(board);
                    var ai = new ImmeWinElseRandom(rule, 2);

                    var player1 = new HumanPlayer(1);
                    Player player2 = isVsComputer ? new ComputerPlayer(ai, 2) : new HumanPlayer(2);

                    var game = new LineUpBasic(board, player1, rule, ai);
                    game.SetPlayer1(player1);
                    game.SetPlayer2(player2);
                    game.InitializeGameloop();
                    game.StartGameLoop();

                    Console.WriteLine("Press Enter to continue...");
                    Console.ReadLine();
                }
                else
                {
                    Console.WriteLine("Invalid selection.");
                    continue;
                }
            }
        }

        static bool PromptVsMode()
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

        protected override void InitializeGameloop()
        {
            _gameOver = false;
            _player1Win = false;
            _player2Win = false;
            AllocateInitialStockByBoardSize();
            PrintHelp();

            // Save initial state for undo/redo
            var initialSnapshot = CaptureGameState(_player1Win, _player2Win, _gameOver);
            MoveManager.SaveState(initialSnapshot);
        }

        protected override bool EndGame() => _gameOver;

        protected override void ExecuteGameTurn()
        {
            if (CurrentPlayer == Player2 && CurrentPlayer.IsComputer)
            {
                var aiMove = AiSrategy.PickMove(Board, Player2);
                if (aiMove == null)
                {
                    _gameOver = true;
                    return;
                }
                ApplyMove(aiMove.Col);
                return;
            }

            int col;
            if (!PromptHumanMove(out col))
            {
                _gameOver = true;
                return;
            }
            ApplyMove(col);
        }

        private void ApplyMove(int col)
        {
            if (!_player1Win && !_player2Win && Board.IsFull())
            {
                _gameOver = true;
                return;
            }

            if (!Board.IsColumnLegal(col))
            {
                Console.WriteLine("Column is full or illegal, please try again.");
                return;
            }

            // Check if player has ordinary disc
            if (!CurrentPlayer.CanUse(DiscKind.Ordinary))
            {
                Console.WriteLine("No discs remaining.");
                return;
            }

            // Consume the disc
            if (!CurrentPlayer.TryConsume(DiscKind.Ordinary))
            {
                Console.WriteLine("Failed to consume disc.");
                return;
            }

            // Create and place the disc
            var disc = DiscFactory.Create(DiscKind.Ordinary, CurrentPlayer);
            var move = new PlaceDiscMove(col, disc);
            move.Execute(Board);
            if (!move.WasPlaced)
            {
                Console.WriteLine("Failed to place a disc. Please retry.");
                // Return the disc back to inventory since placement failed
                CurrentPlayer.AddStock(DiscKind.Ordinary, 1);
                return;
            }

            // Win check
            var rule = (WinRule as ConnectWinRule) ?? new ConnectWinRule(WinLen);
            var change = move.ChangeCells;
            if (change == null)
            {
                return;
            }
            rule.WinCheck(Board, change, out _player1Win, out _player2Win);

            if (_player1Win || _player2Win || Board.IsFull())
            {
                _gameOver = true;
                return;
            }

            SwitchPlayer();
            // Save state AFTER executing move and switching player
            var snapshot = CaptureGameState(_player1Win, _player2Win, _gameOver);
            MoveManager.SaveState(snapshot);
        }

        private void PrintBoard()
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
                        int owner = disc.PlayerId;
                        discSymbol = owner == 1 ? '@' : '#';
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

        private void PrintInventory(Player p)
        {
            Console.WriteLine($"Player {p.playerId} discs remaining: {p.Inventory[DiscKind.Ordinary]}");
        }

        private bool PromptHumanMove(out int col)
        {
            col = -1;

            while (true)
            {
                PrintBoard();
                PrintInventory(CurrentPlayer);
                Console.WriteLine("Enter your move: e.g., 1 for Column 1");
                Console.WriteLine("Commands: Q: quit, H: help, Undo: undo, Redo: redo, Save: save game, Load: load game");
                var line = Console.ReadLine();
                if (string.Equals(line, "q", StringComparison.OrdinalIgnoreCase)) return false;
                if (string.Equals(line, "h", StringComparison.OrdinalIgnoreCase)) { PrintHelp(); continue; }

                // Handle undo
                if (string.Equals(line, "undo", StringComparison.OrdinalIgnoreCase))
                {
                    if (PerformUndo(out _player1Win, out _player2Win, out _gameOver))
                    {
                        return true; // Exit to let game loop continue with restored player
                    }
                    continue;
                }

                // Handle redo
                if (string.Equals(line, "redo", StringComparison.OrdinalIgnoreCase))
                {
                    if (PerformRedo(out _player1Win, out _player2Win, out _gameOver))
                    {
                        return true; // Exit to let game loop continue with restored player
                    }
                    continue;
                }

                // Handle save
                if (string.Equals(line, "save", StringComparison.OrdinalIgnoreCase))
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
                if (string.Equals(line, "load", StringComparison.OrdinalIgnoreCase))
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

                if (string.IsNullOrWhiteSpace(line)) { Console.WriteLine("Empty input"); continue; }

                line = line.Trim();

                if (!int.TryParse(line, out int colInput)) { Console.WriteLine("Invalid column number"); continue; }

                int promptCol = colInput - 1;
                if (promptCol < 0 || promptCol >= Board.Cols) { Console.WriteLine("Column number out of range"); continue; }

                col = promptCol;
                return true;
            }
        }

        protected override void DisplayGameResult()
        {
            PrintBoard();
            Console.WriteLine("Game Over");
            if (_player1Win || _player2Win) WinResult(_player1Win, _player2Win);
            else Console.WriteLine("Draw End");
        }

        private void AllocateInitialStockByBoardSize()
        {
            int totalCells = Board.Rows * Board.Cols;
            int perPlayer = totalCells / 2;

            // Only allocate ordinary discs
            Player1.AddStock(DiscKind.Ordinary, perPlayer);
            Player2.AddStock(DiscKind.Ordinary, perPlayer);
        }
    }
}