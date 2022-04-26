using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChessWebApp.Models.Engine
{
    public class Engine
    {
        const int pawnValue = 100;
        const int knightValue = 300;
        const int bishopValue = 300;
        const int rookValue = 500;
        const int queenValue = 900;

        public static int Evaluate(Board board)
        {
            int whiteEval = CountMaterial(board.TotalPieces, PieceColor.Light);
            int blackEval = CountMaterial(board.TotalPieces, PieceColor.Dark);

            int evaluation = whiteEval - blackEval;

            int perspective = board.IsWhitesMove ? 1 : -1;
            return evaluation * perspective;
        }

        private static int CountMaterial(IEnumerable<AbstractPiece> pieces, PieceColor color)
        {
            int material = 0;
            material += pieces.Where(p => p is Pawn && p.PieceColor == color).Count() * pawnValue;
            material += pieces.Where(p => p is Knight && p.PieceColor == color).Count() * knightValue;
            material += pieces.Where(p => p is Bishop && p.PieceColor == color).Count() * bishopValue;
            material += pieces.Where(p => p is Rook && p.PieceColor == color).Count() * rookValue;
            material += pieces.Where(p => p is Queen && p.PieceColor == color).Count() * queenValue;
            return material;
        }

        private int Search(Board board, int depth)
        {
            if (depth == 0) return Evaluate(board);

            MoveManager moveManager = new();
            var pieceMoves = moveManager.GenerateForAllPieces(board);

            if (pieceMoves.Values.Count() == 0)
            {
                if (board.King.IsInCheck) return int.MinValue;
                return 0;
            }

            int bestEvaluation = int.MinValue;

            foreach (var piece in pieceMoves.Keys)
            {
                foreach (var move in pieceMoves[piece])
                {
                    moveManager.GetValidMoves(board, board.King, board.LocationSquareMap[piece.Location]);
                }
            }


            return 0;
        }
    }
}
