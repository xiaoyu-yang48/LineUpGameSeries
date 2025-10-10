using System;
using System.Linq;
using System.Collections.Generic;

namespace LineUpSeries
{
    // Top-level launcher that lets user pick mode, size, and load saves
    public sealed class LauncherGame : Game
    {
        public override string Name => "Launcher";
        private bool _exit;

        public LauncherGame()
            : base(new Board(1, 1), new HumanPlayer(1), new ConnectWinRule(4), new RandomAIStrategy())
        {
        }

        protected override void InitializeGameLoop()
        {
            _exit = false;
            Console.WriteLine("Welcome to LineUpSeries Launcher");
            PrintMenu();
        }

        protected override bool EndGame() => _exit;

        protected override void ExecuteGameTurn()
        {
            Console.Write("menu> ");
            var input = Console.ReadLine()?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(input)) return;

            var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var cmd = parts[0].ToLowerInvariant();

            switch (cmd)
            {
                case "help":
                case "menu":
                    PrintMenu();
                    break;
                case "new":
                    StartNewFlow();
                    break;
                case "load":
                    if (parts.Length < 2)
                    {
                        Console.WriteLine("Usage: load <file>");
                        break;
                    }
                    StartLoadFlow(parts[1]);
                    break;
                case "quit":
                case "exit":
                    _exit = true;
                    break;
                default:
                    Console.WriteLine("Unknown command. Type 'help'.");
                    break;
            }
        }

        protected override void DisplayGameResult()
        {
            Console.WriteLine("Bye.");
        }

        private void PrintMenu()
        {
            Console.WriteLine("Commands:");
            Console.WriteLine("  new          - start new LineUpClassic game (interactive)");
            Console.WriteLine("  load <file>  - load a saved game");
            Console.WriteLine("  help/menu    - show this menu");
            Console.WriteLine("  quit         - exit application");
        }

        private void StartNewFlow()
        {
            int rows = AskInt("Rows", 6);
            int cols = AskInt("Cols", 7);
            int win = AskInt("Win length", 4);
            bool vsAI = AskBool("Play vs AI? (y/n)", true);

            var winRule = new ConnectWinRule(win);
            IAIStrategy ai = new ImmediateWinOrRandomAIStrategy(winRule, 2);

            Player.SetPlayer1(new HumanPlayer(1));
            Player.SetPlayer2(vsAI ? new ComputerPlayer(2, ai) : new HumanPlayer(2));

            var game = new LineUpClassic(new Board(rows, cols), Player.Player1!, winRule, ai, vsAI);
            game.StartGameLoop();
            Console.WriteLine("Returned to launcher.");
        }

        private void StartLoadFlow(string path)
        {
            try
            {
                var save = FileManager.Load(path);
                var board = new Board(save.Rows, save.Cols);
                FileManager.LoadInto(board, save);

                var winRule = new ConnectWinRule(save.WinLen);
                IAIStrategy ai = new ImmediateWinOrRandomAIStrategy(winRule, 2);

                bool vsAI = save.VsAI;
                Player.SetPlayer1(new HumanPlayer(1));
                Player.SetPlayer2(vsAI ? new ComputerPlayer(2, ai) : new HumanPlayer(2));

                var game = new LineUpClassic(board, Player.Player1!, winRule, ai, vsAI);
                game.StartGameLoop();
                Console.WriteLine("Returned to launcher.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Load failed: {ex.Message}");
            }
        }

        private static int AskInt(string prompt, int defaultValue)
        {
            Console.Write($"{prompt} [{defaultValue}]: ");
            var s = Console.ReadLine();
            if (int.TryParse(s, out var v) && v > 0) return v;
            return defaultValue;
        }

        private static bool AskBool(string prompt, bool defaultValue)
        {
            Console.Write($"{prompt} [{(defaultValue ? "y" : "n")}]: ");
            var s = (Console.ReadLine() ?? string.Empty).Trim().ToLowerInvariant();
            if (s == "y" || s == "yes") return true;
            if (s == "n" || s == "no") return false;
            return defaultValue;
        }
    }
}
