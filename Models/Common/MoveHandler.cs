using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChessWebApp
{
    public class MoveHandler
    {
        private static Board board;
        private static bool IsWhitesMove = true;
        private static Square fromSquare;
        private static Square FromSquare
        {
            get => fromSquare;
            set
            {
                if (value == null) board.ValidMoves.Clear();
                board.LocationSquareMap.Values.ToList().ForEach(sq => sq.IsValid = false);
                fromSquare = value;
            }
        }

        public MoveHandler(Board _board)
        {
            board = _board;
        }

        public static bool PerformMove(Board board, Square square)
        {
            bool isMovePerformed = false;

            Func<bool> FromSquareIsSelected = () => FromSquare != null && FromSquare.CurrentPiece != null;
            Func<bool> SquareIsEmpty = () => square.CurrentPiece == null;

            Func<bool> SelectedAndTargetPieceAreTheSamePiece = ()
                => FromSquare == square && FromSquare.CurrentPiece != null;

            Func<bool> SelectedAndTargetPieceAreTheSameColor = ()
                => FromSquare.CurrentPiece.PieceColor == square.CurrentPiece.PieceColor;

            Func<Square, bool> MoveOrderIsWrong = sq
                 => IsWhitesMove && sq.CurrentPiece.PieceColor != PieceColor.Light
                || !IsWhitesMove && sq.CurrentPiece.PieceColor != PieceColor.Dark;

            if (!FromSquareIsSelected())
            {
                if (!SquareIsEmpty() && !MoveOrderIsWrong(square))
                {
                    FromSquare = square;

                    if (FromSquare.CurrentPiece != null && !MoveOrderIsWrong(FromSquare))
                        board.UpdateValidSquares(FromSquare.CurrentPiece);
                }
            }
            else
            {
                if (MoveOrderIsWrong(FromSquare))
                {
                    FromSquare = null;
                }
                else
                {
                    if (!SquareIsEmpty() && SelectedAndTargetPieceAreTheSamePiece())
                    {
                        FromSquare = null;
                    }
                    else if (!SquareIsEmpty() && SelectedAndTargetPieceAreTheSameColor())
                    {
                        FromSquare = square;
                        board.UpdateValidSquares(FromSquare.CurrentPiece);
                    }
                    else
                    {   //making a move

                        board.UpdateValidSquares(FromSquare.CurrentPiece);

                        if (board.ValidMoves.Contains(square.Location))
                        {
                            FromSquare.CurrentPiece.MakeMove(square);
                            UpdateSquareAttackedByPieces(board, square);
                            IsWhitesMove = !IsWhitesMove;

                            isMovePerformed = true;
                        }
                        FromSquare = null;
                    }
                }
            }
            return isMovePerformed;
        }

        private static void UpdateSquareAttackedByPieces(Board board, Square square)
        {
            board.UpdateValidSquares(square.CurrentPiece);

            foreach (Square sq in board.BoardSquares)
                if (sq.AttackedByPieces.Contains(square.CurrentPiece))
                    sq.AttackedByPieces.Remove(square.CurrentPiece);

            var attackedLocs = square.CurrentPiece.GetLocationsAttackedByPiece(board);

            foreach (Location loc in attackedLocs)
                board.LocationSquareMap[loc].AttackedByPieces.Add(square.CurrentPiece);
        }


    }
}
