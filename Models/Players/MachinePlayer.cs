using ChessWebApp.Models.Common;
using ChessWebApp.Models.Notation;
using Stockfish.NET;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ChessWebApp.Models.Players
{
    public class MachinePlayer : Player
    {
        public int Score { get; set; }
        public List<AbstractPiece> CapturedPieces { get; set; }
        private Engine.MyEngine engine ;
        private Fen fen = new();
        private static IStockfish stockfish;

        public MachinePlayer()
        {
            CapturedPieces = new();
            engine = new Engine.MyEngine();
            stockfish = new Stockfish.NET.Stockfish(@"Models\Engine\stockfish_20090216_x64_avx2.exe");
        }

        public bool TryMakeMove(Game game, MoveManager moveManager, Square square)
        {
            bool isMovePerformed = false;
            Board board = game.Board;

            var positionFen = fen.Parse(board);

            stockfish.SetFenPosition(positionFen);

            string bestMoveString = stockfish.GetBestMove();

            Move bestMove = Move.Parse(bestMoveString);

            isMovePerformed = moveManager.MakeMove(
                board,
                board.LocationSquareMap[bestMove.From],
                board.LocationSquareMap[bestMove.To]);

            return isMovePerformed;
        }

        //public bool TryMakeMove(Game game, MoveManager moveManager, Square square)
        //{
        //    bool isMovePerformed = false;
        //    Board board = game.Board;

        //    Move bestMove = engine.GetBestMove(board);

        //    isMovePerformed = moveManager.MakeMove(
        //        board,
        //        board.LocationSquareMap[bestMove.From],
        //        board.LocationSquareMap[bestMove.To]);

        //    return isMovePerformed;
        //}
    }
}
