using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace LineUpSeries
{
    public sealed class LineUpBasic : Game
    {
        public override string Name => "LineUpBasic";

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

                    // Fixed board size: 8 x 9 for LineUpBasic
                    var board = new Board(8, 9);
                    var rule = new ConnectWinRule(4);
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
                ApplyMove(aiMove.Col, DiscKind.Ordinary);
                return;
            }

            int col;
            DiscKind kind;
            string cmd;
            if (!PromptHumanMove(out col, out kind, out cmd))
            {
                _gameOver = true;
                return;
            }
            // In LineUpBasic, always use Ordinary disc regardless of input
            ApplyMove(col, DiscKind.Ordinary);
        }

        /// Override to customize for LineUpBasic - only accepts column number
        protected override bool ParseMoveInput(string? line, out int col, out DiscKind kind)
        {
            col = -1;
            kind = DiscKind.Ordinary; // Always ordinary in Basic mode

            if (string.IsNullOrWhiteSpace(line))
            {
                Console.WriteLine("Empty input");
                return false;
            }

            line = line.Trim();

            if (!int.TryParse(line, out int colInput))
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

            col = promptCol;
            return true;
        }

        /// Override to customize the move prompt for LineUpBasic
        protected override void PrintMovePrompt()
        {
            PrintInventory(CurrentPlayer);
            Console.WriteLine("Enter your move: e.g., 1 for Column 1");
            Console.WriteLine("Commands: Q: quit, H: help, Undo: undo, Redo: redo, Save: save game, Load: load game");
        }

        /// Override to show only ordinary disc count
        protected override void PrintInventory(Player p)
        {
            Console.WriteLine($"Player {p.playerId} discs remaining: {p.Inventory[DiscKind.Ordinary]}");
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