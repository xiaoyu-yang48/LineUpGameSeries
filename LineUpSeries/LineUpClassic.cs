using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace LineUpSeries
{
    public class LineUpClassic : Game
    {
        public override string Name => "LineUpClassic";
        private readonly Random _random = new Random();

        public LineUpClassic(Board board, Player currentPlayer, IWinRule winRule, IAIStrategy aiSrategy) : base(board, currentPlayer, winRule, aiSrategy)
        {
        }

        //default constructor
        public LineUpClassic() : base()
        {
        }

        public override void Launch()
        {
            while (true)
            {
                Console.WriteLine("==== LineUpClassic ====");
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
                    (int rows, int cols) = PromptBoardSize();

                    var board = new Board(rows, cols);
                    var rule = new ConnectWinRule(4);
                    rule.SetWinLen(board);
                    var ai = new ImmeWinElseRandom(rule, 2);

                    var player1 = new HumanPlayer(1);
                    Player player2 = isVsComputer ? new ComputerPlayer(ai, 2) : new HumanPlayer(2);

                    var game = new LineUpClassic(board, player1, rule, ai);
                    game.SetPlayer1(player1);
                    game.SetPlayer2(player2);
                    game.InitializeGameloop();
                    game.StartGameLoop();

                    Console.WriteLine("Enter q to quit");
                    var cont = Console.ReadLine();
                    if (string.Equals(cont, "q", StringComparison.OrdinalIgnoreCase)) return;
                }
                else
                {
                    Console.WriteLine("Invalid selection.");
                    continue;
                }
            }
        }

        private static (int rows, int cols) PromptBoardSize()
        {
            const int minRows = 6;
            const int minCols = 7;
            int rows = 0;
            int cols = 0;

            while (true)
            {
                try
                {
                    Console.WriteLine($"Please enter your board rows: (>= {minRows})");
                    rows = int.Parse(Console.ReadLine()?.Trim());
                    Console.WriteLine($"Please enter your board columns: (>= {minCols}), and rows <= columns");
                    cols = int.Parse(Console.ReadLine()?.Trim());

                    if (rows < minRows)
                        throw new ArgumentOutOfRangeException($"Rows must be >= {minRows}");
                    if (cols < minCols)
                        throw new ArgumentOutOfRangeException($"Columns must be >= {minCols}");
                    if (rows > cols)
                        throw new ArgumentOutOfRangeException("Rows cannot exceed columns.");

                    break;
                }
                catch (ArgumentNullException)
                {
                    Console.WriteLine("Your input was null.");
                }
                catch (FormatException)
                {
                    Console.WriteLine("Your input was not a valid integer.");
                }
                catch (OverflowException)
                {
                    Console.WriteLine("Your number is too big or small");
                }
                catch (ArgumentOutOfRangeException e)
                {
                    Console.WriteLine(e.Message);
                }
                catch (Exception)
                {
                    Console.WriteLine("Invalid input");
                }
            }
            return (rows, cols);
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
                ApplyMove(aiMove.Col, aiMove.Disc.Kind);
                return;
            }
            int col;
            DiscKind kind;
            if (!PromptHumanMove(out col, out kind, out _))
            {
                _gameOver = true;
                return;
            }
            ApplyMove(col, kind);
        }

        protected virtual void AllocateInitialStockByBoardSize()
        {
            int totalCells = Board.Rows * Board.Cols;
            int perPlayer = totalCells / 2;
            var specials = new List<DiscKind> { DiscKind.Boring, DiscKind.Magnetic, DiscKind.Explosive };

            var p1s1 = specials[_random.Next(specials.Count)];
            DiscKind p1s2;
            do { p1s2 = specials[_random.Next(specials.Count)]; } while (p1s2 == p1s1);
            AllocateFor(Player1, perPlayer, p1s1, p1s2);

            var p2s1 = specials[_random.Next(specials.Count)];
            DiscKind p2s2;
            do { p2s2 = specials[_random.Next(specials.Count)]; } while (p2s2 == p2s1);
            AllocateFor(Player2, perPlayer, p2s1, p2s2);

            void AllocateFor(Player p, int total, DiscKind s1, DiscKind s2)
            {
                int specialsTotal = 2 + 2;
                int ordinary = Math.Max(0, total - specialsTotal);
                p.AddStock(DiscKind.Ordinary, ordinary);
                p.AddStock(s1, 2);
                p.AddStock(s2, 2);
            }
        }

    }
}
