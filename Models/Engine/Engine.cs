using ChessWebApp.Models.Common;
using ChessWebApp.Models.Notation;
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

        private int Search(Board board, int depth, int alpha, int beta)
        {
            if (depth <= 0) return Evaluate(board);

            MoveManager moveManager = new();
            var pieceMoves = moveManager.GenerateForAllPieces(board);

            if (pieceMoves.Values.Count() == 0)
            {
                if (board.King.IsInCheck) return int.MinValue;
                return 0;
            }

            foreach (var piece in pieceMoves.Keys)
            {
                foreach (Location move in pieceMoves[piece])
                {
                    //make move, count evaluation, bring board position back
                    MoveManager moveManager1 = new();
                    Board fakeBoard = board.Copy();

                    moveManager1.MakeMove(
                        fakeBoard,
                        fakeBoard.LocationSquareMap[piece.Location],
                        fakeBoard.LocationSquareMap[move]);

                    int evaluation = -Search(fakeBoard, depth - 1, -beta, -alpha);

                    if (evaluation >= beta)
                        //Move was too good, opponent will avoid this position
                        return beta;    // Snip

                    alpha = Math.Max(alpha, evaluation);
                }
            }

            return alpha;
        }

        public void OrderMoves(List<Move> moves)
        {
            foreach (Move move in moves)
            {
                int moveScoreGuess = 0;
                AbstractPiece pieceToMove = move.From.CurrentPiece;
                AbstractPiece pieceToCapture = move.To.CurrentPiece;

                //Prioritise capturing opponent's most valuable piece with least valuable piece
                if (pieceToCapture is not null)
                    moveScoreGuess = 10 * GetPieceValue(pieceToCapture) - GetPieceValue(pieceToMove);

                //promoting a pawn is likely to be good
                if (pieceToMove is Pawn pawn && pawn.IsPromotingNextMove)
                    moveScoreGuess += queenValue;

                //penalize moving our piece to a square attacked by opponent pawn
                if (move.To.AttackedByPiecesOnSquares
                    .Select(sq => sq.CurrentPiece)
                    .Where(p => p is Pawn && p.PieceColor != pieceToMove.PieceColor)
                    .Count() > 0)
                    moveScoreGuess -= GetPieceValue(pieceToMove);
            }
        }

        private int GetPieceValue(AbstractPiece capturePiece)
        {
            throw new NotImplementedException();
        }
    }
}
