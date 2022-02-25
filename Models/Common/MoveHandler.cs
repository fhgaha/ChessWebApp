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

        public static void CalculateLegalMoves(Board board, Square square)
        {
            Func<bool> FromSquareIsSelected = () => fromSquare != null && fromSquare.CurrentPiece != null;
            Func<bool> SquareIsEmpty = () => square.CurrentPiece == null;

            Func<bool> SelectedAndTargetPieceAreTheSamePiece = ()
                => fromSquare == square && fromSquare.CurrentPiece != null;

            Func<bool> SelectedAndTargetPieceAreTheSameColor = ()
                => fromSquare.CurrentPiece.PieceColor == square.CurrentPiece.PieceColor;

            Func<bool> MoveOrderIsWrong = ()
                 => IsWhitesMove && fromSquare.CurrentPiece.PieceColor != PieceColor.Light
                || !IsWhitesMove && fromSquare.CurrentPiece.PieceColor != PieceColor.Dark;

            if (!FromSquareIsSelected())
            {
                fromSquare = square;

                if (fromSquare.CurrentPiece != null && !MoveOrderIsWrong())
                    board.UpdateValidSquares(fromSquare.CurrentPiece);
            }
            else
            {
                if (MoveOrderIsWrong())
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
                    }
                }
                board.ValidMoves.Clear();
            }
        }

        private static void UpdateSquareAttackedByPieces(Board board, Square square)
        {
            board.UpdateValidSquares(square.CurrentPiece);

            foreach (Square sq in board.BoardSquares)
            {
                if (sq.AttackedByPieces.Contains(square.CurrentPiece))
                    sq.AttackedByPieces.Remove(square.CurrentPiece);
            }

            foreach (Square sq in board.BoardSquares)
            {
                foreach (Location loc in board.ValidMoves)
                {
                    if (sq.Location == loc)
                    {
                        //if (square.CurrentPiece is Pawn)
                        //{
                        //    var rankOffset = square.CurrentPiece.PieceColor == PieceColor.Light ? 1 : -1;

                        //    var attackedLocation = LocationFactory.Build(square.Location, -1, rankOffset);
                        //    if (attackedLocation != null)
                        //        board.LocationSquareMap[attackedLocation].AttackedByPieces.Add(square.CurrentPiece);
                            
                        //    attackedLocation = LocationFactory.Build(square.Location, 1, rankOffset);
                        //    if (attackedLocation != null)
                        //        board.LocationSquareMap[attackedLocation].AttackedByPieces.Add(square.CurrentPiece);

                        //}
                        //else
                        //{
                        //    sq.AttackedByPieces.Add(square.CurrentPiece);
                        //}

                        var attackedLocs = square.CurrentPiece.GetLocationsAttackedByPiece();

                        foreach (Location key in board.LocationSquareMap.Keys)
                        {
                            foreach (Location l in attackedLocs)
                            {
                                if (key == l)
                                {
                                    board.LocationSquareMap[key].AttackedByPieces.Add(square.CurrentPiece);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
