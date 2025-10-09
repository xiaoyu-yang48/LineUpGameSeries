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
        private bool _vsAI;
        private bool _gameOver;
        private bool _player1Win;
        private bool _player2Win;
        private DiscKind _currentDiscKind = DiscKind.Ordinary;

        public LineUpClassic(Board board, Player currentPlayer, IWinRule winRule, IAIStrategy aiStrategy, bool vsAI)
            : base(board, currentPlayer, winRule, aiStrategy)
        {
            _vsAI = vsAI;
        }

        protected override void InitializeGameLoop()
        {
            _gameOver = false;
            _player1Win = false;
            _player2Win = false;
            var stock = new Dictionary<DiscKind, int>
            {
                { DiscKind.Ordinary, 42 },
                { DiscKind.Boring, 0 },
                { DiscKind.Magnetic, 0 },
                { DiscKind.Explosive, 0 },
            };
            Player.Player1.SetInventory(stock);
            Player.Player2.SetInventory(stock);
            Console.WriteLine($"Game: {Name} | {Board.Rows}x{Board.Cols}, win {WinLen}");
            PrintHelp();
            PrintBoard();
        }

        protected override bool EndGame() => _gameOver;

        protected override void ExecuteGameTurn()
        {
            if (CurrentPlayer == Player.Player2 && _vsAI)
            {
                int aiCol = AiStrategy.ChooseMove(Board);
                if (aiCol < 0)
                {
                    _gameOver = true;
                    return;
                }
                // If AI strategy suggests a piece kind, adopt it for this move
                if (AiStrategy is ImmediateWinOrRandomAIStrategy smart)
                {
                    _currentDiscKind = smart.LastChosenDiscKind;
                }
                ApplyMove(aiCol, CurrentPlayer.PlayerId);
                return;
            }

            Console.Write($"Player {CurrentPlayer.PlayerId}, enter column (0-{Board.Cols-1}) or command: ");
            var input = Console.ReadLine()?.Trim() ?? string.Empty;
            if (HandleCommand(input)) return;

            if (!int.TryParse(input, out var col))
            {
                Console.WriteLine("Invalid input. Type 'help' for commands.");
                return;
            }
            ApplyMove(col, CurrentPlayer.PlayerId);
        }

        private void ApplyMove(int col, int playerId)
        {
            if (!Board.IsLegalMove(col))
            {
                Console.WriteLine("Illegal move: column full or out of range.");
                return;
            }

            // decide piece kind per side (AI sets this before ApplyMove via strategy)
            var kindToUse = _currentDiscKind;

            // check inventory
            var player = playerId == 1 ? Player.Player1 : Player.Player2;
            if (!player.TryConsume(kindToUse))
            {
                Console.WriteLine($"No stock for piece kind '{kindToUse}'. Use 'piece <kind>' or restock.");
                return;
            }

            var move = new PlaceDiscMove(col, DiscFactory.Create(kindToUse, playerId));
            move.Execute(Board);
            Board.ApplyGravity();

            WinCheck(move.ChangeCells);
            PrintBoard();

            if (_player1Win || _player2Win || Board.IsFull())
            {
                _gameOver = true;
                return;
            }
            SwitchPlayer();
        }

        private void WinCheck(ChangeCell changeCells)
        {
            // include the placed cell if missing
            if (changeCells.Cells.Count == 0)
            {
                // fallback scan for last disc per column is omitted; rely on OnPlaced to add
            }
            if (changeCells != null)
            {
                // compute wins for changed cells
                (WinRule as ConnectWinRule ?? new ConnectWinRule(WinLen))
                    .WinCheck(Board, changeCells, out _player1Win, out _player2Win);
            }

            if (!_player1Win && !_player2Win)
            {
                // fallback: full board scan, to capture gravity-induced wins
                FullBoardWinScan(out _player1Win, out _player2Win);
            }
        }

        private void FullBoardWinScan(out bool p1, out bool p2)
        {
            p1 = false; p2 = false;
            for (int r = 0; r < Board.Rows; r++)
            {
                for (int c = 0; c < Board.Cols; c++)
                {
                    var cell = Board.Cells[r][c];
                    if (cell.Disc == null) continue;
                    if (WinRule.CheckCellWin(Board, cell))
                    {
                        if (cell.Disc.PlayerId == 1) p1 = true; else p2 = true;
                        if (p1 && p2) return;
                    }
                }
            }
        }

        protected override void DisplayGameResult()
        {
            if (_player1Win || _player2Win)
            {
                WinResult(_player1Win, _player2Win);
            }
            else
            {
                Console.WriteLine("Game over: draw (board full or no moves).");
            }
        }

        private bool HandleCommand(string input)
        {
            var lower = input.ToLowerInvariant();
            switch (lower)
            {
                case "help":
                    PrintHelp();
                    return true;
                case "board":
                    PrintBoard();
                    return true;
                case "stock":
                    PrintStock();
                    return true;
                case "quit":
                    _gameOver = true;
                    return true;
            }

            if (lower.StartsWith("save "))
            {
                var path = input.Substring(5).Trim();
                if (string.IsNullOrWhiteSpace(path))
                {
                    Console.WriteLine("Usage: save <file>");
                    return true;
                }
                try
                {
                    FileManager.Save(path, Board, WinLen, CurrentPlayer.PlayerId, _vsAI);
                    Console.WriteLine($"Saved to {path}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Save failed: {ex.Message}");
                }
                return true;
            }
            if (lower.StartsWith("load "))
            {
                var path = input.Substring(5).Trim();
                if (string.IsNullOrWhiteSpace(path))
                {
                    Console.WriteLine("Usage: load <file>");
                    return true;
                }
                try
                {
                    var save = FileManager.Load(path);
                    FileManager.LoadInto(Board, save);
                    Console.WriteLine($"Loaded from {path}");
                    PrintBoard();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Load failed: {ex.Message}");
                }
                return true;
            }
            if (lower.StartsWith("piece "))
            {
                var kind = input.Substring(6).Trim();
                if (Enum.TryParse<DiscKind>(kind, true, out var k))
                {
                    _currentDiscKind = k;
                    Console.WriteLine($"Next piece: {_currentDiscKind}");
                }
                else
                {
                    Console.WriteLine("Unknown piece kind. Use: ordinary|boring|magnetic|explosive");
                }
                return true;
            }
            if (lower.StartsWith("restock "))
            {
                // restock <kind> <amount> (debug/assist tool)
                var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 3 && Enum.TryParse<DiscKind>(parts[1], true, out var k) && int.TryParse(parts[2], out var amt))
                {
                    var p = CurrentPlayer.PlayerId == 1 ? Player.Player1 : Player.Player2;
                    p.AddStock(k, amt);
                    Console.WriteLine($"Player {p.PlayerId} stock {k} += {amt}");
                }
                else
                {
                    Console.WriteLine("Usage: restock <kind> <amount>");
                }
                return true;
            }
            return false;
        }

        private void PrintHelp()
        {
            Console.WriteLine("Commands: [number]=drop column | help | board | stock | piece <kind> | restock <kind> <amount> | save <file> | load <file> | quit");
        }

        private void PrintBoard()
        {
            for (int r = Board.Rows - 1; r >= 0; r--)
            {
                for (int c = 0; c < Board.Cols; c++)
                {
                    var owner = Board.Cells[r][c].Owner;
                    char ch = owner == 0 ? '.' : (owner == 1 ? 'X' : 'O');
                    Console.Write(ch);
                    Console.Write(' ');
                }
                Console.WriteLine();
            }
        }

        private void PrintStock()
        {
            void Print(Player p)
            {
                Console.Write($"P{p.PlayerId} stock: ");
                foreach (DiscKind k in Enum.GetValues(typeof(DiscKind)))
                {
                    p.Inventory.TryGetValue(k, out var cnt);
                    Console.Write($"{k}={cnt} ");
                }
                Console.WriteLine();
            }
            Print(Player.Player1);
            Print(Player.Player2);
        }

        
    }
}
