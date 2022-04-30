using System;
using System.Collections.Generic;

namespace ChessWebApp.Models.Players
{
    public class HumanPlayer : Player
    {
        public int Score { get; set; }
        public List<AbstractPiece> CapturedPieces { get; set; }
        public bool TryMakeMove(Game game, MoveManager moveManager, Square square)
        {
            bool isMovePerformed = false;
            Board board = game.Board;
            Square fromSquare = game.FromSquare;

            Func<bool> FromSquareIsSelected = () => game.FromSquare != null && game.FromSquare.CurrentPiece != null;
            Func<bool> SquareIsEmpty = () => square.CurrentPiece == null;
            Func<bool> SelectedAndTargetPieceAreTheSamePiece = ()
                => game.FromSquare == square && game.FromSquare.CurrentPiece != null;
            Func<bool> SelectedAndTargetPieceAreTheSameColor = ()
                => game.FromSquare.CurrentPiece.PieceColor == square.CurrentPiece.PieceColor;
            Func<Square, bool> MoveOrderIsWrong = sq
                 => board.IsWhitesMove && sq.CurrentPiece.PieceColor != PieceColor.Light
                || !board.IsWhitesMove && sq.CurrentPiece.PieceColor != PieceColor.Dark;

            if (!FromSquareIsSelected())
            {
                if (SquareIsEmpty())
                    game.FromSquare = null;
                else if (MoveOrderIsWrong(square))
                    game.FromSquare = null;
                else
                {
                    game.FromSquare = square;
                    //moveManager.UpdateValidSquares(board, board.King, square);
                }
            }
            else if (FromSquareIsSelected())
            {
                if (MoveOrderIsWrong(fromSquare))
                    game.FromSquare = null;
                else if (!SquareIsEmpty() && SelectedAndTargetPieceAreTheSamePiece())
                    game.FromSquare = null;
                else if (!SquareIsEmpty() && SelectedAndTargetPieceAreTheSameColor())
                {
                    game.FromSquare = square;
                    //moveManager.UpdateValidSquares(board, board.King, fromSquare);
                }
                //selected and moving is allowed
                else if (moveManager.GetValidMoves(board, board.King, fromSquare).Contains(square.Location))  
                {
                    isMovePerformed = moveManager.MakeMove(board, fromSquare, square);

                    game.FromSquare = null;
                }
                else
                    game.FromSquare = null;
            }
            else
                game.FromSquare = null;

            return isMovePerformed;

        }
    }
}
