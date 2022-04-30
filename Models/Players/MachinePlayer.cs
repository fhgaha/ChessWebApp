using ChessWebApp.Models.Common;
using System.Collections.Generic;

namespace ChessWebApp.Models.Players
{
    public class MachinePlayer : Player
    {
        public int Score { get; set; }
        public List<AbstractPiece> CapturedPieces { get; set; }
        private Engine.Engine engine ;

        public MachinePlayer()
        {
            CapturedPieces = new();
            engine = new Engine.Engine();
        }

        public bool TryMakeMove(Game game, MoveManager moveManager, Square square)
        {
            bool isMovePerformed = false;
            Board board = game.Board;

            Move bestMove = engine.GetBestMove(board);

            isMovePerformed = moveManager.MakeMove(
                board,
                board.LocationSquareMap[bestMove.From],
                board.LocationSquareMap[bestMove.To]);

            return isMovePerformed;
        }
    }
}
