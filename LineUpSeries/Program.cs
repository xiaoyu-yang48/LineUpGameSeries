namespace LineUpSeries
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Start();
        }

        public static void Start()
        {
            // ensure save directory exists
            const string SaveDirectory = "saves";
            if (!System.IO.Directory.Exists(SaveDirectory))
            {
                System.IO.Directory.CreateDirectory(SaveDirectory);
            }

            Console.WriteLine("Welcome to Line Up!");
            LineUpClassic? game = null;

            // main menu
            while (true)
            {
                Console.WriteLine("Enter 1 = start a new game; 2 = load saved game; 3 = exit");
                var choice = Console.ReadLine()?.Trim();

                if (choice == "1")
                {
                    var (rows, cols, winLen) = SetBoardSize();
                    Console.WriteLine($"Your game board is {rows} * {cols}, WinLen = {winLen}");

                    bool vsComputer = ReadGameMode();

                    var winRule = new ConnectWinRule(winLen);
                    IAIStrategy ai = new ImmediateWinOrRandomAIStrategy(winRule, 2);
                    Player.SetPlayer1(new HumanPlayer(1));
                    Player.SetPlayer2(vsComputer ? new ComputerPlayer(2, ai) : new HumanPlayer(2));

                    game = new LineUpClassic(new Board(rows, cols), Player.Player1!, winRule, ai, vsComputer);
                    break;
                }
                else if (choice == "2")
                {
                    var loaded = LoadGame();
                    if (loaded != null)
                    {
                        game = loaded;
                        Console.WriteLine("Game loaded successfully!");
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Failed to load game.");
                    }
                }
                else if (choice == "3")
                {
                    Console.WriteLine("Thanks for playing! See you next time");
                    return;
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter 1, 2, or 3.");
                }
            }

            if (game == null)
            {
                Console.WriteLine("Failed to initialize game.");
                return;
            }

            game.StartGameLoop();
        }

        private static (int rows, int cols, int win) SetBoardSize()
        {
            int rows = ReadInt("Enter rows", 6, 1, 99);
            int cols = ReadInt("Enter cols", 7, 1, 99);
            int win = ReadInt("Enter win length", 4, 2, Math.Max(rows, cols));
            return (rows, cols, win);
        }

        private static bool ReadGameMode()
        {
            while (true)
            {
                Console.WriteLine("Select mode: 1 = human vs human, 2 = human vs computer");
                var v = Console.ReadLine()?.Trim();
                if (v == "1") return false;
                if (v == "2") return true;
                Console.WriteLine("Invalid mode. Try again.");
            }
        }

        private static int ReadInt(string prompt, int defaultVal, int min, int max)
        {
            while (true)
            {
                Console.Write($"{prompt} [{defaultVal}]: ");
                var s = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(s)) return defaultVal;
                if (int.TryParse(s, out var v) && v >= min && v <= max) return v;
                Console.WriteLine($"Please enter an integer between {min} and {max}.");
            }
        }

        private static LineUpClassic? LoadGame()
        {
            try
            {
                Console.WriteLine("Enter save filename (under 'saves' directory):");
                var name = Console.ReadLine()?.Trim();
                if (string.IsNullOrWhiteSpace(name)) return null;
                var path = System.IO.Path.Combine("saves", name);
                var save = FileManager.Load(path);
                var board = new Board(save.Rows, save.Cols);
                FileManager.LoadInto(board, save);
                var winRule = new ConnectWinRule(save.WinLen);
                IAIStrategy ai = new ImmediateWinOrRandomAIStrategy(winRule, 2);
                bool vsComputer = save.VsAI;
                Player.SetPlayer1(new HumanPlayer(1));
                Player.SetPlayer2(vsComputer ? new ComputerPlayer(2, ai) : new HumanPlayer(2));
                return new LineUpClassic(board, Player.Player1!, winRule, ai, vsComputer);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Load failed: {ex.Message}");
                return null;
            }
        }
    }
}
