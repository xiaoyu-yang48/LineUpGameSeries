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

        public LineUpClassic(Board board, Player currentPlayer, IWinRule winRule, IAIStrategy aiSrategy, bool isVsComputer) : base(board, currentPlayer, winRule, aiSrategy)
        {
            _isVsComputer = isVsComputer;
        }

        protected override void InitializeGameloop()
        {
            _gameOver = false;
            _player1Win = false;
            _player2Win = false;
            SetBoardSize();
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
                if (aiMove != null)
                {
                    _gameOver = true;
                    return;
                }
                ApplyMove(aiMove.Column, CurrentPlayer.PlayerId, aiMove.Disc.Kind);
                return;
            }


        }

        private void ApplyMove(int col, int playerId, DiscKind kindToUse)
        { 
        }


    }
}
