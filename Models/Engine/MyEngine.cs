using ChessWebApp.Models.MoveModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;

namespace ChessWebApp.Models.Engine
{
    public class MyEngine
    {
        const int pawnValue = 100;
        const int knightValue = 300;
        const int bishopValue = 300;
        const int rookValue = 500;
        const int queenValue = 900;

        const int depth = 4;

        private MoveManager moveManager;

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

        Move bestMove = new Move { Score = 0 };

        //with order: 71809 evals, took 00:01:50.9678175
        //with no order: 71809 evals, took 00:01:51.0505512
        List<int> evaluations = new();  

        public Move GetBestMove(Board board)
        {
            Stopwatch timer = new();
            moveManager = new();
            timer.Start();
            Maximizer(board, depth, int.MinValue, int.MaxValue);
            timer.Stop();
            var t = timer.Elapsed;
            movesWithScores = movesWithScores.OrderByDescending(m => m.Score).ToList();
            return bestMove;
        }

        List<Move> movesWithScores = new();
        private int Maximizer(Board board, int depth, int alpha, int beta)
        {
            if (depth == 0) return Evaluate(board);

            List<Move> moves = new MoveManager().GenerateMovesForAllPieces(
                board, board.IsWhitesMove ? PieceColor.Light : PieceColor.Dark);

            if (moves.Count() == 0)
            {
                if (board.King.IsInCheck)
                    return int.MinValue;    //?
                return 0;
            }

            foreach (Move move in moves)
            {
                //make move, count evaluation, bring board position back
                
                int evaluation = 0;
                //evaluation += OrderMove(board, move);

                var isMoveSuccessfull = moveManager.MakeMove(
                    board,
                    board.LocationSquareMap[move.From],
                    board.LocationSquareMap[move.To]);

                evaluation = Minimizer(board, depth - 1, alpha, beta);
                evaluations.Add(evaluation);
                moveManager.UndoMove(board);

                if (depth == MyEngine.depth)
                {
                    move.Score = evaluation;
                    movesWithScores.Add(move);
                }

                if (evaluation > alpha)
                {
                    alpha = evaluation;

                    if (depth == MyEngine.depth)
                        bestMove = move;
                }

                if (alpha >= beta)
                    return alpha;
            }

            return alpha;
        }

        private int Minimizer(Board board, int depth, int alpha, int beta)
        {
            if (depth == 0) return Evaluate(board);

            List<Move> moves = new MoveManager().GenerateMovesForAllPieces(
                board, board.IsWhitesMove ? PieceColor.Light : PieceColor.Dark);

            if (moves.Count() == 0)
            {
                if (board.King.IsInCheck)
                    return int.MinValue;    //?
                return 0;
            }

            foreach (Move move in moves)
            {
                //make move, count evaluation, bring board position back
                int evaluation = 0;
                //evaluation = OrderMove(board, move);

                var isMoveSuccessfull = moveManager.MakeMove(
                    board,
                    board.LocationSquareMap[move.From],
                    board.LocationSquareMap[move.To]);

                evaluation = Maximizer(board, depth - 1, alpha, beta);
                evaluations.Add(evaluation);
                moveManager.UndoMove(board);

                if (evaluation <= beta)
                    beta = evaluation;

                if (alpha >= beta)
                    return beta;
            }
            return beta;
        }

        public int OrderMove(Board board, Move move)
        {
            int moveScoreGuess = 0;
            Square fromSq = board.LocationSquareMap[move.From];
            Square toSq = board.LocationSquareMap[move.To];
            AbstractPiece pieceToMove = fromSq.CurrentPiece;
            AbstractPiece pieceToCapture = toSq.CurrentPiece;

            //Prioritise capturing opponent's most valuable piece with least valuable piece
            if (pieceToCapture is not null)
            {
                int pieceToMoveVal = GetPieceValue(pieceToMove);
                int pieceToCaptureVal = GetPieceValue(pieceToCapture);
                moveScoreGuess = pieceToCaptureVal - pieceToMoveVal;
            }

            //promoting a pawn is likely to be good
            if (pieceToMove is Pawn pawn && pawn.IsPromotingNextMove)
                moveScoreGuess += queenValue;

            //penalize moving our piece to a square attacked by opponent pawn
            if (toSq.AttackedByPiecesOnSquares
                .Select(sq => sq.CurrentPiece)
                .Where(p => p is Pawn && p.PieceColor != pieceToMove.PieceColor)
                .Count() > 0)
                moveScoreGuess -= GetPieceValue(pieceToMove);

            //develop pieces
            //if (pieceToMove.IsFirstMove)
            //{
            //    if (pieceToMove is Pawn) moveScoreGuess += 4;
            //    if (pieceToMove is Knight || pieceToMove is Bishop) moveScoreGuess += 3;
            //    if (pieceToMove is Rook) moveScoreGuess += 2;
            //    if (pieceToMove is Queen) moveScoreGuess += 1;
            //}

            return moveScoreGuess;
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
