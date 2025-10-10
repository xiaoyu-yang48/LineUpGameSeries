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
        // Entry launcher providing menu and game setup for LineUpClassic
        public static void Run()
        {
            while (true)
            {
                Console.WriteLine("==== LineUpClassic ====");
                Console.WriteLine("1) New Game");
                Console.WriteLine("2) Exit");
                Console.Write("Select: ");
                var pick = Console.ReadLine();
                if (pick == null) return;
                pick = pick.Trim();
                if (string.Equals(pick, "2", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(pick, "q", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(pick, "quit", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(pick, "exit", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
                if (!string.Equals(pick, "1", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Unknown option. Please pick 1 or 2.");
                    continue;
                }

                bool isVsComputer = PromptVsMode();
                (int rows, int cols) = PromptBoardSize();

                var board = new Board(rows, cols);
                var rule = new ConnectWinRule(4);
                rule.SetWinLen(board);
                var ai = new ImmeWinElseRandom(rule, 2);

                Player.SetPlayer1(new HumanPlayer(1));
                if (isVsComputer)
                {
                    Player.SetPlayer2(new ComputerPlayer(ai, 2));
                }
                else
                {
                    Player.SetPlayer2(new HumanPlayer(2));
                }

                var game = new LineUpClassic(board, Player.Player1, rule, ai, isVsComputer);
                game.StartGameLoop();

                Console.WriteLine();
                Console.WriteLine("Press ENTER to return to menu or 'q' to quit.");
                var cont = Console.ReadLine();
                if (string.Equals(cont, "q", StringComparison.OrdinalIgnoreCase)) return;
                Console.Clear();
            }

            static bool PromptVsMode()
            {
                while (true)
                {
                    Console.WriteLine("Select Mode:");
                    Console.WriteLine("1) Human vs Human");
                    Console.WriteLine("2) Human vs Computer");
                    Console.Write("Mode: ");
                    var m = Console.ReadLine();
                    if (m == null) return false;
                    m = m.Trim();
                    if (m == "1") return false;
                    if (m == "2") return true;
                    Console.WriteLine("Unknown option. Please pick 1 or 2.");
                }
            }

            static (int rows, int cols) PromptBoardSize()
            {
                const int defaultRows = 6;
                const int defaultCols = 7;
                while (true)
                {
                    Console.Write($"Enter board size (rows cols), default {defaultRows} {defaultCols}: ");
                    var line = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(line)) return (defaultRows, defaultCols);
                    var parts = line.Trim().Split(new[] { ' ', '\t', ',', 'x', 'X' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2 && int.TryParse(parts[0], out int r) && int.TryParse(parts[1], out int c))
                    {
                        r = Math.Clamp(r, 4, 20);
                        c = Math.Clamp(c, 4, 20);
                        return (r, c);
                    }
                    Console.WriteLine("Invalid size. Example: 6 7");
                }
            }
        }

        public abstract string Name { get; }
        public Board Board { get; }
        public Player CurrentPlayer { get; private set; } = Player.Player1;

        protected readonly IWinRule WinRule;
        protected readonly IAIStrategy AiSrategy;
        public int WinLen => WinRule.WinLen;

        protected Game (Board board, Player currentPlayer, IWinRule winRule, IAIStrategy aiSrategy)
        {
            Board = board;
            CurrentPlayer = currentPlayer;
            WinRule = winRule;
            AiSrategy = aiSrategy;
        }

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
