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
        // Top-level launcher with game mode selection
        public static void Run()
        {
            while (true)
            {
                Console.WriteLine("==== LineUp Series ====");
                Console.WriteLine("1) Classic");
                Console.WriteLine("2) Basic");
                Console.WriteLine("3) Spin");
                Console.WriteLine("4) Exit");
                Console.Write("Select Game Mode: ");
                var pick = Console.ReadLine();
                if (pick == null) return;
                pick = pick.Trim();

                if (pick == "4" || pick.Equals("q", StringComparison.OrdinalIgnoreCase) || pick.Equals("quit", StringComparison.OrdinalIgnoreCase) || pick.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
                else if (pick == "1")
                {
                    LaunchClassic();
                }
                else if (pick == "2")
                {
                    Console.WriteLine("LineUpBasic 暂未实现，按回车返回。");
                    Console.ReadLine();
                    Console.Clear();
                }
                else if (pick == "3")
                {
                    Console.WriteLine("LineUpSpin 暂未实现，按回车返回。");
                    Console.ReadLine();
                    Console.Clear();
                }
                else
                {
                    Console.WriteLine("无效选项，请输入 1-4。");
                }
            }

            static void LaunchClassic()
            {
                while (true)
                {
                    Console.WriteLine("==== LineUpClassic ====");
                    Console.WriteLine("1) New Game");
                    Console.WriteLine("2) Back");
                    Console.Write("Select: ");
                    var sel = Console.ReadLine();
                    if (sel == null) return;
                    sel = sel.Trim();
                    if (sel == "2") return;
                    if (sel != "1")
                    {
                        Console.WriteLine("未知选项，请输入 1 或 2。");
                        continue;
                    }

                    bool isVsComputer = PromptVsMode();
                    (int rows, int cols) = PromptBoardSize();

                    var board = new Board(rows, cols);
                    var rule = new ConnectWinRule(4);
                    rule.SetWinLen(board);
                    var ai = new ImmeWinElseRandom(rule, 2);

                    Player.SetPlayer1(new HumanPlayer(1));
                    if (isVsComputer) Player.SetPlayer2(new ComputerPlayer(ai, 2));
                    else Player.SetPlayer2(new HumanPlayer(2));

                    var game = new LineUpClassic(board, Player.Player1, rule, ai, isVsComputer);
                    game.StartGameLoop();

                    Console.WriteLine();
                    Console.WriteLine("按回车返回 Classic 菜单，或输入 'q' 直接退出到主菜单。");
                    var cont = Console.ReadLine();
                    if (string.Equals(cont, "q", StringComparison.OrdinalIgnoreCase)) return;
                    Console.Clear();
                }
            }

            static bool PromptVsMode()
            {
                while (true)
                {
                    Console.WriteLine("选择对战模式:");
                    Console.WriteLine("1) 人人对战");
                    Console.WriteLine("2) 人机对战");
                    Console.Write("Mode: ");
                    var m = Console.ReadLine();
                    if (m == null) return false;
                    m = m.Trim();
                    if (m == "1") return false;
                    if (m == "2") return true;
                    Console.WriteLine("未知选项，请输入 1 或 2。");
                }
            }

            static (int rows, int cols) PromptBoardSize()
            {
                const int defaultRows = 6;
                const int defaultCols = 7;
                while (true)
                {
                    Console.Write($"请输入棋盘尺寸 (行 列)，默认 {defaultRows} {defaultCols}: ");
                    var line = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(line)) return (defaultRows, defaultCols);
                    var parts = line.Trim().Split(new[] { ' ', '\t', ',', 'x', 'X' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2 && int.TryParse(parts[0], out int r) && int.TryParse(parts[1], out int c))
                    {
                        r = Math.Clamp(r, 4, 20);
                        c = Math.Clamp(c, 4, 20);
                        return (r, c);
                    }
                    Console.WriteLine("尺寸格式错误，例如: 6 7");
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
