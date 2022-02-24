using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChessWebApp.Services
{
    public class MoveChecker
    {

        public bool IsMovePossible(Board board, Square fromSquare, Square square, ref bool IsWhitesMove)
        {
            Func<bool> FromSquareIsSelected = () => fromSquare != null && fromSquare.CurrentPiece != null;
            Func<bool> SquareIsEmpty = () => square.CurrentPiece == null;

            Func<bool> SelectedAndTargetPieceAreTheSamePiece = ()
                => fromSquare == square && fromSquare.CurrentPiece != null;

            Func<bool> SelectedAndTargetPieceColorsAreTheSame = ()
                => fromSquare.CurrentPiece.PieceColor == square.CurrentPiece.PieceColor;

            if (!FromSquareIsSelected())
            {
                fromSquare = square;
                board.UpdateValidSquares(fromSquare.CurrentPiece);
            }
            else
            {
                if (IsWhitesMove && fromSquare.CurrentPiece.PieceColor != PieceColor.Light
                || !IsWhitesMove && fromSquare.CurrentPiece.PieceColor != PieceColor.Dark)
                {
                    fromSquare = null;
                    return false;
                }

                if (SelectedAndTargetPieceAreTheSamePiece())
                {
                    fromSquare = null;
                }
                else
                {
                    if (!SquareIsEmpty() && SelectedAndTargetPieceColorsAreTheSame())
                    {
                        fromSquare = square;
                        board.UpdateValidSquares(fromSquare.CurrentPiece);
                    }
                    else
                    {
                        fromSquare.CurrentPiece.MakeMove(square);
                        //fromSquare.Reset();
                        //board.LocationSquareMap[fromSquare.Location].Reset();
                        fromSquare = null;
                        IsWhitesMove = !IsWhitesMove;
                    }
                }
            }
            return true;
        }
    }
}
