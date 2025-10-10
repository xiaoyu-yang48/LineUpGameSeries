using System;

namespace LineUpSeries
{
    internal class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("==== LineUpClassic ====");
                Console.WriteLine("1) 新游戏");
                Console.WriteLine("2) 退出");
                Console.Write("选择: ");
                var input = Console.ReadLine()?.Trim();

                if (string.Equals(input, "2", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(input, "q", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                if (!string.Equals(input, "1", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("无效选择，请重试。");
                    continue;
                }

                bool isVsComputer = AskMode();
                var (rows, cols) = AskBoardSize();

                var board = new Board(rows, cols);
                var winRule = new ConnectWinRule(4);
                winRule.SetWinLen(board);
                IAIStrategy aiStrategy = new ImmeWinElseRandom(winRule, 2);

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
                Console.WriteLine("按回车返回主菜单，或输入 q 退出");
                var after = Console.ReadLine();
                if (string.Equals(after, "q", StringComparison.OrdinalIgnoreCase)) return;
            }
        }

        private static bool AskMode()
        {
            while (true)
            {
                Console.WriteLine("选择模式: 1) 人人  2) 人机");
                Console.Write("选择: ");
                var input = Console.ReadLine()?.Trim();
                if (string.Equals(input, "1", StringComparison.OrdinalIgnoreCase)) return false; // 人人对战
                if (string.Equals(input, "2", StringComparison.OrdinalIgnoreCase)) return true;  // 人机对战
                Console.WriteLine("无效选择，请输入 1 或 2。");
            }
        }

        private static (int rows, int cols) AskBoardSize()
        {
            while (true)
            {
                Console.WriteLine("设置棋盘尺寸 (行 列)，例如 6 7；直接回车使用默认 6x7");
                Console.Write("尺寸: ");
                var line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) return (6, 7);

                var parts = line.Split(new[] { 'x', 'X', ' ', '*', ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2 && int.TryParse(parts[0], out int r) && int.TryParse(parts[1], out int c))
                {
                    if (r >= 4 && c >= 4 && r <= 20 && c <= 20)
                    {
                        return (r, c);
                    }
                    Console.WriteLine("行列需介于 4 到 20 之间。");
                }
                else
                {
                    Console.WriteLine("格式错误，请按示例输入，如 6 7 或 6x7。");
                }
            }
        }
    }
}
