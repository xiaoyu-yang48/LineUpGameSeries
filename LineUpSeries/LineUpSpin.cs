using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace LineUpSeries
{
    public class LineUpSpin : LineUpClassic
    {
        public override string Name => "LineUpSpin";
        private int _turnCounter = 0;

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

        // --- Only ordinary discs ---
        protected override void AllocateInitialStockByBoardSize()
        {
            // number of available discs
            int totalCells = Board.Rows * Board.Cols;
            int perPlayer = totalCells / 2;

            // only implement ordinary discs 
            Player1.AddStock(DiscKind.Ordinary, perPlayer);
            Player2.AddStock(DiscKind.Ordinary, perPlayer);
        }

        // --- Prompt Move ---
        protected override bool PromptHumanMove(out int col, out DiscKind kind)
        {
            col = -1;
            kind = DiscKind.Ordinary;

            while (true)
            {
                Console.WriteLine("Enter your move (column number only). Every 5 turns the board spins!");
                Console.WriteLine("Commands: Q: quit, Undo, Redo");

                var line = Console.ReadLine();
                if (string.Equals(line, "q", StringComparison.OrdinalIgnoreCase))
                    return false;

                if (string.Equals(line, "undo", StringComparison.OrdinalIgnoreCase))
                {
                    if (PerformUndo(out _, out _, out _))
                    {
                        PrintBoard();
                        return true;
                    }
                    continue;
                }

                if (string.Equals(line, "redo", StringComparison.OrdinalIgnoreCase))
                {
                    if (PerformRedo(out _, out _, out _))
                    {
                        PrintBoard();
                        return true;
                    }
                    continue;
                }

                if (string.IsNullOrWhiteSpace(line))
                {
                    Console.WriteLine("Empty input.");
                    continue;
                }

                if (!int.TryParse(line.Trim(), out int colInput))
                {
                    Console.WriteLine("Invalid column number.");
                    continue;
                }

                col = colInput - 1;
                if (col < 0 || col >= Board.Cols)
                {
                    Console.WriteLine("Column number out of range.");
                    continue;
                }

                kind = DiscKind.Ordinary;
                return true;
            }
        }

        // --- clockwise rotation logic every 5 turns ---
        protected override void ExecuteGameTurn()
        {
            base.ExecuteGameTurn();  // Run the normal turn logic

            // If the game is already over, skip rotation
            if (Board == null || Board.IsFull()) return;

            _turnCounter++;
            if (_turnCounter % 5 == 0)
            {
                Console.WriteLine("\n*** The board spins! ***\n");
                Board.RotateCW();
                Board.ApplyGravity();
                PrintBoard();
                // Save a snapshot after rotation so Undo can restore it
                var snapshot = CaptureGameState(false, false, false);
                MoveManager.SaveState(snapshot);

            }
        }





    }
}


