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
                    LineUpClassic.Launch();
                }
                else if (pick == "2")
                {
                    LineUpBasic.Launch();
                }
                else if (pick == "3")
                {
                    LineUpSpin.Launch();
                }
                else
                {
                    Console.WriteLine("无效选项，请输入 1-4。");
                }
            }
        }
        public abstract string Name { get; }
        public Board Board { get; }
        public Player CurrentPlayer { get; private set; } = Player.Player1;

        protected readonly IWinRule WinRule;
        protected readonly IAIStrategy AiSrategy;
        public int WinLen => WinRule.WinLen;

        // Hook for subclass-specific launching flow (optional override)
        public virtual void Launch()
        {
            Console.WriteLine("该模式尚未提供实例化启动入口。");
        }

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
