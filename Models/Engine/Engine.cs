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


        public Move GetBestMove(Board board, int depth, int alpha, int beta)
        {
            Search(board, depth, alpha, beta);

            return new Move
            {
                From = board.LocationSquareMap[bestPiece.Location],
                To = board.LocationSquareMap[bestTo],
                Score = bestScore,
                CapturedPiece = board.LocationSquareMap[bestTo].CurrentPiece,
                MovingPiece = bestPiece,
            };
        }

        private AbstractPiece bestPiece;
        private Location bestTo;
        private int bestScore;

        private int Search(Board board, int depth, int alpha, int beta)
        {
            if (depth <= 0) return Evaluate(board);

            MoveManager moveManager = new();
            var pieceMoves = moveManager.GeneratePossibleMovesForAllPieces(board);

            if (pieceMoves.Values.Count() == 0)
            {
                if (board.King.IsInCheck) 
                    return int.MinValue;
                return 0;
            }

            foreach (var piece in pieceMoves.Keys)
                foreach (Location move in pieceMoves[piece])
                {
                    //make move, count evaluation, bring board position back
                    MoveManager _moveManager = new();
                    Board _board = board.Copy();

                    _moveManager.MakeMove(
                        _board,
                        _board.LocationSquareMap[piece.Location],
                        _board.LocationSquareMap[move]);

                    int evaluation = -Search(_board, depth - 1, -beta, -alpha);

                    if (evaluation >= beta)
                        //Move was too good, opponent will avoid this position
                        return beta;    // Snip

                    alpha = Math.Max(alpha, evaluation);

                    if (alpha > bestScore)
                    {
                        bestPiece = piece;
                        bestScore = alpha;
                        bestTo = move;
                    }
                }

            return alpha;
        }

        public int OrderMoves(List<Move> moves)
        {
            List<int> moveScores = new();

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

                moveScores.Add(moveScoreGuess);
            }
            return moveScores.Max();
        }

        private int GetPieceValue(AbstractPiece piece)
        {
            switch (piece)
            {
                case Pawn: return pawnValue;
                case Knight: return knightValue;
                case Bishop: return bishopValue;
                case Rook: return rookValue;
                default: return queenValue;
            }
        }
    }
}
