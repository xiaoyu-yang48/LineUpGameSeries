namespace LineUpSeries
{
    internal class Program
    {
        static void Main(string[] args)
        {
            RunCli();
        }

        private static void RunCli()
        {
            Console.WriteLine("LineUpSeries CLI");
            Console.WriteLine("Type 'help' to see available commands.");

            Game? currentGame = null;
            while (true)
            {
                Console.Write("main> ");
                var line = Console.ReadLine();
                if (line == null) break;
                var input = line.Trim();
                if (string.IsNullOrEmpty(input)) continue;

                var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var cmd = parts[0].ToLowerInvariant();
                var argsRest = parts.Skip(1).ToArray();

                switch (cmd)
                {
                    case "help":
                        PrintMainHelp();
                        break;
                    case "new":
                        // new [rows] [cols] [win] [mode=human|ai]
                        int rows = argsRest.Length > 0 && int.TryParse(argsRest[0], out var r) ? r : 6;
                        int cols = argsRest.Length > 1 && int.TryParse(argsRest[1], out var c) ? c : 7;
                        int win = argsRest.Length > 2 && int.TryParse(argsRest[2], out var w) ? w : 4;
                        bool vsAI = argsRest.Length > 3 ? argsRest[3].Equals("ai", StringComparison.OrdinalIgnoreCase) : false;
                        var winRule = new ConnectWinRule(win);
                        IAIStrategy ai = new ImmediateWinOrRandomAIStrategy(winRule, 2);
                        // set players: P1 human, P2 human/AI depending on mode
                        Player.SetPlayer1(new HumanPlayer(1));
                        Player.SetPlayer2(vsAI ? new ComputerPlayer(2, ai) : new HumanPlayer(2));
                        currentGame = new LineUpClassic(new Board(rows, cols), Player.Player1, winRule, ai, vsAI);
                        currentGame.StartGameLoop();
                        currentGame = null;
                        break;
                    case "quit":
                    case "exit":
                        return;
                    default:
                        Console.WriteLine("Unknown command. Type 'help'.");
                        break;
                }
            }
        }

        private static void PrintMainHelp()
        {
            Console.WriteLine("Main commands:");
            Console.WriteLine("  new [rows] [cols] [win] [human|ai] - start LineUpClassic game");
            Console.WriteLine("  help - show this help");
            Console.WriteLine("  quit - exit application");
        }
    }
}
