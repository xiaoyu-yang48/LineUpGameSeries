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

        // Entry menu and setup flow
        public static void Start()
        {
            while (true)
            {
                Console.WriteLine("==== LineUpClassic ====");
                Console.WriteLine("1) 新游戏");
                Console.WriteLine("2) 退出");
                Console.Write("请选择 (1/2): ");
                var choice = Console.ReadLine();
                if (string.Equals(choice, "2", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                if (!string.Equals(choice, "1", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("无效选择，请重试。\n");
                    continue;
                }

                bool isVsComputer = AskVsMode();
                (int rows, int cols) = AskBoardSize();

                // setup players
                var winRule = new ConnectWinRule(4);
                IAIStrategy aiStrategy = new ImmeWinElseRandom(winRule);

                var board = new Board(rows, cols);

                Player.SetPlayer1(new HumanPlayer(1));
                if (isVsComputer)
                {
                    Player.SetPlayer2(new ComputerPlayer(aiStrategy, 2));
                }
                else
                {
                    Player.SetPlayer2(new HumanPlayer(2));
                }

                var game = new LineUpClassic(board, Player.Player1, winRule, aiStrategy, isVsComputer);
                game.StartGameLoop();

                Console.WriteLine();
                Console.WriteLine("按回车返回主菜单，或输入 2 退出。");
                var back = Console.ReadLine();
                if (string.Equals(back, "2", StringComparison.OrdinalIgnoreCase)) return;
            }
        }

        private static bool AskVsMode()
        {
            while (true)
            {
                Console.WriteLine("选择模式: 1) 人人对战  2) 人机对战");
                Console.Write("请选择 (1/2): ");
                var mode = Console.ReadLine();
                if (string.Equals(mode, "1", StringComparison.OrdinalIgnoreCase)) return false;
                if (string.Equals(mode, "2", StringComparison.OrdinalIgnoreCase)) return true;
                Console.WriteLine("无效选择，请重试。\n");
            }
        }

        private static (int rows, int cols) AskBoardSize()
        {
            int rows = ReadPositiveInt("请输入棋盘行数(>=4，默认6): ", 6, min: 4, max: 50);
            int cols = ReadPositiveInt("请输入棋盘列数(>=4，默认7): ", 7, min: 4, max: 50);
            return (rows, cols);
        }

        private static int ReadPositiveInt(string prompt, int defaultValue, int min = 1, int max = 100)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(s)) return defaultValue;
                if (int.TryParse(s, out int val) && val >= min && val <= max) return val;
                Console.WriteLine($"请输入范围内的整数[{min}, {max}]，或直接回车使用默认值 {defaultValue}。\n");
            }
        }
    }
}
