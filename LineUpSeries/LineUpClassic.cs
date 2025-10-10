using System;
using System.Collections.Generic;
using System.Linq;
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
            if (CurrentPlayer == Player.Player2 && _isVsComputer)
            {
                var aiMove = AiSrategy.PickMove(Board);
                if (aiMove == null)
                {
                    _gameOver = true;
                    return;
                }
                Console.WriteLine($"电脑选择列 {aiMove.Col + 1}，棋子 {aiMove.Disc.Kind}。");
                ApplyMove(aiMove.Col, CurrentPlayer.playerId, aiMove.Disc.Kind);
                return;
            }

            // human move
            Console.WriteLine($"轮到玩家 {CurrentPlayer.playerId}。");
            PrintInventory(CurrentPlayer);
            int col;
            DiscKind kind;
            if (!PromptHumanMove(out col, out kind))
            {
                _gameOver = true;
                return;
            }
            ApplyMove(col, CurrentPlayer.playerId, kind);
        }

        private void ApplyMove(int col, int playerId, DiscKind kindToUse)
        {
            if (!_player1Win && !_player2Win && Board.IsFull())
            {
                _gameOver = true;
                return;
            }

            if (!Board.IsColumnLegal(col))
            {
                Console.WriteLine("该列已满或非法，请重试。");
                return;
            }

            var player = Player.GetById(playerId)!;
            var disc = DiscFactory.Create(kindToUse, playerId);
            if (!Board.IsDiscLegal(disc))
            {
                Console.WriteLine("该类型棋子库存不足，请选择其他类型。");
                return;
            }

            var move = new PlaceDiscMove(col, disc);
            move.Execute(Board);
            if (!move.WasPlaced)
            {
                Console.WriteLine("落子失败，请重试。");
                return;
            }

            // consume stock after successful placement
            player.TryConsume(kindToUse);

            // apply gravity if special effects created empty spaces
            Board.ApplyGravity();

            // win check
            var rule = (WinRule as ConnectWinRule) ?? new ConnectWinRule(WinLen);
            var change = move.ChangeCells;
            if (change == null || change.Cells.Count == 0)
            {
                change = BuildAllNonEmptyChange();
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

        private ChangeCell BuildAllNonEmptyChange()
        {
            var cc = new ChangeCell();
            for (int r = 0; r < Board.Rows; r++)
            {
                for (int c = 0; c < Board.Cols; c++)
                {
                    var cell = Board.Cells[r][c];
                    if (!cell.IsEmpty) cc.Add(cell);
                }
            }
            return cc;
        }

        private void AllocateInitialStockByBoardSize()
        {
            int totalCells = Board.Rows * Board.Cols;
            int perPlayer = totalCells / 2;
            var specials = new List<DiscKind> { DiscKind.Boring, DiscKind.Magnetic, DiscKind.Explosive };

            // Player 1 picks two specials
            var p1s1 = specials[_random.Next(specials.Count)];
            DiscKind p1s2;
            do { p1s2 = specials[_random.Next(specials.Count)]; } while (p1s2 == p1s1);
            AllocateFor(Player.Player1, perPlayer, p1s1, p1s2);

            // Player 2 picks two specials (independent random)
            var p2s1 = specials[_random.Next(specials.Count)];
            DiscKind p2s2;
            do { p2s2 = specials[_random.Next(specials.Count)]; } while (p2s2 == p2s1);
            AllocateFor(Player.Player2, perPlayer, p2s1, p2s2);

            void AllocateFor(Player p, int total, DiscKind s1, DiscKind s2)
            {
                int specialsTotal = 2 + 2; // two of each
                int ordinary = Math.Max(0, total - specialsTotal);
                p.AddStock(DiscKind.Ordinary, ordinary);
                p.AddStock(s1, 2);
                p.AddStock(s2, 2);
            }
        }

        private void PrintHelp()
        {
            Console.WriteLine("玩法: 连接4子获胜。");
            Console.WriteLine("输入示例: 1b 表示第1列投放 Boring 棋子；支持 o/b/m/x。");
            Console.WriteLine("输入 q 退出当前对局，输入 h 查看帮助。");
        }

        private void PrintBoard()
        {
            Console.WriteLine("当前棋盘:");
            for (int r = Board.Rows - 1; r >= 0; r--)
            {
                var sb = new System.Text.StringBuilder();
                for (int c = 0; c < Board.Cols; c++)
                {
                    var cell = Board.Cells[r][c];
                    if (cell.IsEmpty) sb.Append(" . ");
                    else sb.Append(cell.Owner == 1 ? " 1 " : " 2 ");
                }
                Console.WriteLine(sb.ToString());
            }
            var footer = new System.Text.StringBuilder();
            for (int c = 1; c <= Board.Cols; c++) footer.Append($" {c,2}");
            Console.WriteLine(footer.ToString());
        }

        private void PrintInventory(Player p)
        {
            Console.WriteLine($"库存 P{p.playerId}: Ordinary={p.Inventory[DiscKind.Ordinary]}, Boring={p.Inventory[DiscKind.Boring]}, Magnetic={p.Inventory[DiscKind.Magnetic]}, Explosive={p.Inventory[DiscKind.Explosive]}");
        }

        private bool PromptHumanMove(out int col, out DiscKind kind)
        {
            col = -1;
            kind = DiscKind.Ordinary;
            while (true)
            {
                Console.Write("请输入落子，如 1b（列+类型：o/b/m/x），q 退出，h 帮助: ");
                var line = Console.ReadLine();
                if (string.Equals(line, "q", StringComparison.OrdinalIgnoreCase)) return false;
                if (string.Equals(line, "h", StringComparison.OrdinalIgnoreCase)) { PrintHelp(); PrintBoard(); continue; }
                if (string.IsNullOrWhiteSpace(line)) { Console.WriteLine("输入为空。"); continue; }

                line = line.Trim();
                // 提取末尾字母作为棋子类型，其余为列号
                char last = line[line.Length - 1];
                if (!char.IsLetter(last)) { Console.WriteLine("格式应为 数字+字母，例如 1b。"); continue; }
                string numPart = line.Substring(0, line.Length - 1);
                if (!int.TryParse(numPart, out int colInput)) { Console.WriteLine("列号无效。"); continue; }

                int colZero = colInput - 1;
                if (colZero < 0 || colZero >= Board.Cols) { Console.WriteLine("列号越界。"); continue; }

                char kch = char.ToLowerInvariant(last);
                kind = kch switch
                {
                    'o' => DiscKind.Ordinary,
                    'b' => DiscKind.Boring,
                    'm' => DiscKind.Magnetic,
                    'x' => DiscKind.Explosive,
                    _ => DiscKind.Ordinary
                };

                col = colZero;
                return true;
            }
        }

        protected override void DisplayGameResult()
        {
            Console.WriteLine("游戏结束。");
            if (_player1Win || _player2Win)
            {
                WinResult(_player1Win, _player2Win);
            }
            else
            {
                Console.WriteLine("棋盘已满，平局。");
            }
        }


    }
}
