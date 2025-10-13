using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace LineUpSeries
{
    public sealed class LineUpClassic : Game
    {
        public override string Name => "LineUpClassic";
        private bool _isVsComputer;
        private bool _gameOver;
        private bool _player1Win;
        private bool _player2Win;
        private readonly Random _random = new Random();

        public LineUpClassic(Board board, Player currentPlayer, IWinRule winRule, IAIStrategy aiSrategy, bool isVsComputer) : base(board, currentPlayer, winRule, aiSrategy)
        {
            _isVsComputer = isVsComputer;
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

                    var game = new LineUpClassic(board, player1, rule, ai, isVsComputer);
                    game.SetPlayer1(player1);
                    game.SetPlayer2(player2);
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
        static bool PromptVsMode()
        {
            while (true)
            {
                Console.WriteLine("1: human vs human");
                Console.WriteLine("2: human vs computer");
                Console.WriteLine("Choose palyer mode:");

                var mode = Console.ReadLine();
                if (mode == null) return false;
                mode = mode.Trim();
                if (mode == "1") return false;
                if (mode == "2") return true;
                Console.WriteLine("Invalid input");
            }
        }

        private static (int rows, int cols) PromptBoardSize()
        {
            const int minRows = 8;
            const int minCols = 9;
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
            PrintBoard();
        }

        protected override bool EndGame() => _gameOver;

        protected override void ExecuteGameTurn()
        {
            if (CurrentPlayer == Player2 && _isVsComputer)
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
            Console.WriteLine($"Your turn.");
            PrintInventory(CurrentPlayer);
            int col;
            DiscKind kind;
            if (!PromptHumanMove(out col, out kind))
            {
                _gameOver = true;
                return;
            }
            ApplyMove(col, kind);

        }

        private void ApplyMove(int col, DiscKind kindToUse)
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


            // Check if player has the disc kind before consuming
            if (!CurrentPlayer.CanUse(kindToUse))
            {
                Console.WriteLine("Insufficient disc type. Please choose another disc kind.");
                return;
            }

            // Consume the disc first
            if (!CurrentPlayer.TryConsume(kindToUse))
            {
                Console.WriteLine("Failed to consume disc.");
                return;
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
                return;
            }

            //wincheck
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
                PrintBoard();
                return;
            }

            SwitchPlayer();
            PrintBoard();
        }

        private void PrintHelp()
        {
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
                        };
                    }

                    else discSymbol = ' ';


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
            Console.WriteLine($"stock P{p.playerId}: Ordinary = {p.Inventory[DiscKind.Ordinary]}, Boring = {p.Inventory[DiscKind.Boring]}, Magnetic = {p.Inventory[DiscKind.Magnetic]}, Explosive = {p.Inventory[DiscKind.Explosive]}");
        }

        private bool PromptHumanMove(out int col, out DiscKind kind)
        {
            col = -1;
            kind = DiscKind.Ordinary;

            while (true)
            {
                Console.WriteLine("Enter your move: e.g., 1B for BoringDisc in Column 1, Q: quit, H: help");
                var line = Console.ReadLine();
                if (string.Equals(line, "q", StringComparison.OrdinalIgnoreCase)) return false;
                if (string.Equals(line, "h", StringComparison.OrdinalIgnoreCase)) { PrintHelp(); PrintBoard(); continue; }
                if (string.IsNullOrWhiteSpace(line)) { Console.WriteLine("Empty input"); continue;}

                line = line.Trim();

                char last = line[line.Length - 1];
                if (!char.IsLetter(last)) { Console.WriteLine("Wrong format"); continue; }
                string numPart = line.Substring(0, line.Length - 1);
                if (!int.TryParse(numPart, out int colInput)) { Console.WriteLine("Invalid column number"); continue; }

                int promptCol = colInput - 1;
                if (promptCol < 0 || promptCol >= Board.Cols) { Console.WriteLine("Column number out of range"); continue; }

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
        }

        protected override void DisplayGameResult()
        {
            Console.WriteLine("Game Over");
            if (_player1Win || _player2Win) WinResult(_player1Win, _player2Win);
            else Console.WriteLine("Draw End");
        }

        private void AllocateInitialStockByBoardSize()
        {
            int totalCells = Board.Rows * Board.Cols;
            int perPlayer = totalCells / 2;
            var specials = new List<DiscKind> { DiscKind.Boring, DiscKind.Magnetic, DiscKind.Explosive};

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
