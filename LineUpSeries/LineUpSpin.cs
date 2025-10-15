using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace LineUpSeries
{
    public class LineUpSpin : Game
    {
        public override string Name => "LineUpSpin";
        public LineUpSpin(Board board, Player currentPlayer, IWinRule winRule, IAIStrategy aiSrategy) : base(board, currentPlayer, winRule, aiSrategy)
        {
        }

        public LineUpSpin() : base() { }

        public override void Launch()
        {
            while (true)
            {
                Console.WriteLine("==== LineUpSpin ====");
                Console.WriteLine("1: New Game");
                Console.WriteLine("2: Back");
                Console.Write("Select: ");
                var sel = Console.ReadLine()?.Trim();

                if (sel == "2") return;
                if (sel == "1")
                {
                    bool isVsComputer = PromptVsMode();

                    const int rows = 8;
                    const int cols = 9;

                    var board = new Board(rows, cols);
                    var rule = new ConnectWinRule(4);
                    rule.SetWinLen(board);
                    var ai = new ImmeWinElseRandom(rule, 2);

                    var player1 = new HumanPlayer(1);
                    Player player2 = isVsComputer ? new ComputerPlayer(ai, 2) : new HumanPlayer(2);

                    var game = new LineUpSpin(board, player1, rule, ai);
                    game.SetPlayer1(player1);
                    game.SetPlayer2(player2);
                    game.InitializeGameloop();
                    game.StartGameLoop();

                    Console.WriteLine("Enter q to quit");
                    var cont = Console.ReadLine();
                    if (string.Equals(Console.ReadLine(), "q", StringComparison.OrdinalIgnoreCase))
                        return;
                }
                else
                {
                    Console.WriteLine("Invalid selection.");
                    continue;
                }
            }
        }

        // Game template method implementations ---
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

        // --- Only ordinary discs ---
        private void AllocateInitialStockByBoardSize()
        {
            // number of available discs
            int totalCells = Board.Rows * Board.Cols;
            int perPlayer = totalCells / 2;

            // only implement ordinary discs 
            Player1.AddStock(DiscKind.Ordinary, perPlayer);
            Player2.AddStock(DiscKind.Ordinary, perPlayer);
        }

        // --- Customize move prompt for LineUpSpin ---
        protected override void PrintMovePrompt()
        {
            PrintInventory(CurrentPlayer);
            Console.WriteLine("Enter your move (column number only). Every 5 turns the board spins!");
            Console.WriteLine("Commands: Q: quit, H: help, Undo: undo, Redo: redo, Save: save game, Load: load game");
        }

        // Customize input parsing to only accept column number 
        protected override bool ParseMoveInput(string? line, out int col, out DiscKind kind)
        {
            col = -1;
            kind = DiscKind.Ordinary; // Always Ordinary for LineUpSpin

            if (string.IsNullOrWhiteSpace(line))
            {
                Console.WriteLine("Empty input.");
                return false;
            }

            if (!int.TryParse(line.Trim(), out int colInput))
            {
                Console.WriteLine("Invalid column number.");
                return false;
            }

            col = colInput - 1;
            if (col < 0 || col >= Board.Cols)
            {
                Console.WriteLine("Column number out of range.");
                return false;
            }

            return true;
        }

        // --- clockwise rotation logic every 5 turns ---
        protected override void ExecuteGameTurn()
        {
            string cmd = "";
            // Execute player move
            if (CurrentPlayer == Player2 && CurrentPlayer.IsComputer)
            {
                var aiMove = AiSrategy.PickMove(Board, Player2);
                if (aiMove == null)
                {
                    _gameOver = true;
                    return;
                }
                ApplyMove(aiMove.Col, aiMove.Disc.Kind);
            }
            else
            {
                int col;
                DiscKind kind;
                if (!PromptHumanMove(out col, out kind, out cmd))
                {
                    _gameOver = true;
                    return;
                }
                ApplyMove(col, kind);
            }

            // If the game is already over, skip rotation
            if (Board == null || Board.IsFull()) return;
            Console.WriteLine(cmd.ToLower());

            if (TurnNumber % 5 == 0)
            {
                Console.Write("\n*** Before the board spins: ***\n\n");
                PrintBoard();
                Console.WriteLine("\n*** The board spins! ***\n");
                Board.RotateCW();
                Board.ApplyGravity();
            }
        }
    }
}


