using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChessWebApp
{
    public class MoveHandler
    {
        private static bool IsWhitesMove = true;
        private static Square fromSquare;

        public static void PerformMove(Board board, Square square)
        {
            Func<bool> FromSquareIsSelected = () => fromSquare != null && fromSquare.CurrentPiece != null;
            Func<bool> SquareIsEmpty = () => square.CurrentPiece == null;

            Func<bool> SelectedAndTargetPieceAreTheSamePiece = ()
                => fromSquare == square && fromSquare.CurrentPiece != null;

            Func<bool> SelectedAndTargetPieceAreTheSameColor = ()
                => fromSquare.CurrentPiece.PieceColor == square.CurrentPiece.PieceColor;

            Func<Square, bool> MoveOrderIsWrong = sq
                 => IsWhitesMove && sq.CurrentPiece.PieceColor != PieceColor.Light
                || !IsWhitesMove && sq.CurrentPiece.PieceColor != PieceColor.Dark;

            if (!FromSquareIsSelected())
            {
                if (!SquareIsEmpty() && !MoveOrderIsWrong(square))
                {
                    fromSquare = square;

                    if (fromSquare.CurrentPiece != null && !MoveOrderIsWrong(fromSquare))
                        board.UpdateValidSquares(fromSquare.CurrentPiece);
                }
            }
            else
            {
                if (MoveOrderIsWrong(fromSquare))
                {
                    fromSquare = null;
                }
                else
                {
                    if (!SquareIsEmpty() && SelectedAndTargetPieceAreTheSamePiece())
                    {
                        fromSquare = null;
                        board.ValidMoves.Clear();
                    }
                    else if (!SquareIsEmpty() && SelectedAndTargetPieceAreTheSameColor())
                    {
                        fromSquare = square;
                        board.UpdateValidSquares(fromSquare.CurrentPiece);
                    }
                    else
                    {   //making a move

                        board.UpdateValidSquares(fromSquare.CurrentPiece);

                        if (board.ValidMoves.Contains(square.Location))
                        {
                            fromSquare.CurrentPiece.MakeMove(square);
                            UpdateSquareAttackedByPieces(board, square);
                            IsWhitesMove = !IsWhitesMove;
                        }
                        fromSquare = null;
                        board.ValidMoves.Clear();
                    }
                }
            }
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
