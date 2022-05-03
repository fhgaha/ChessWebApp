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

            Func<bool> FromSquareIsSelected = () => game.FromSquare != null && game.FromSquare.CurrentPiece != null;
            Func<bool> SquareIsEmpty = () => square.CurrentPiece == null;
            Func<bool> SelectedAndTargetPieceAreTheSamePiece = ()
                => game.FromSquare == square && game.FromSquare.CurrentPiece != null;
            Func<bool> SelectedAndTargetPieceAreTheSameColor = ()
                => game.FromSquare.CurrentPiece.PieceColor == square.CurrentPiece.PieceColor;
            Func<Square, bool> MoveOrderIsWrong = sq
                 => board.IsWhitesMove && sq.CurrentPiece.PieceColor != PieceColor.Light
                || !board.IsWhitesMove && sq.CurrentPiece.PieceColor != PieceColor.Dark;
            Func<bool> MoveIsNotValid = () 
                => moveManager.GetValidMoves(board, board.King, game.FromSquare).Contains(square.Location) == false;

            //from square is not selected
            if (!FromSquareIsSelected() && SquareIsEmpty()) 
            { game.FromSquare = null; return isMovePerformed; }

            if (!FromSquareIsSelected() && MoveOrderIsWrong(square))
            { game.FromSquare = null; return isMovePerformed; }

            if (!FromSquareIsSelected())
            { game.FromSquare = square; return isMovePerformed; }

            //from square is selected
            if (MoveOrderIsWrong(game.FromSquare)) 
            { game.FromSquare = null; return isMovePerformed; }

            if (!SquareIsEmpty() && SelectedAndTargetPieceAreTheSamePiece())
            { game.FromSquare = null; return isMovePerformed; }

            if (!SquareIsEmpty() && SelectedAndTargetPieceAreTheSameColor())
            { game.FromSquare = square; return isMovePerformed; }

            if (MoveIsNotValid())
            { game.FromSquare = null; return isMovePerformed; }

            //all good we can make a move
            isMovePerformed = moveManager.MakeMove(board, game.FromSquare, square);
            game.FromSquare = null;
            return isMovePerformed;

            #region old if else statement
            //if (!FromSquareIsSelected())
            //{
            //    if (SquareIsEmpty())
            //        game.FromSquare = null;
            //    else if (MoveOrderIsWrong(square))
            //        game.FromSquare = null;
            //    else
            //        game.FromSquare = square;
            //}
            //else if (FromSquareIsSelected())
            //{
            //    if (MoveOrderIsWrong(fromSquare))
            //        game.FromSquare = null;
            //    else if (!SquareIsEmpty() && SelectedAndTargetPieceAreTheSamePiece())
            //        game.FromSquare = null;
            //    else if (!SquareIsEmpty() && SelectedAndTargetPieceAreTheSameColor())
            //    {
            //        game.FromSquare = square;
            //    }
            //    //selected and moving is allowed
            //    else if (moveManager.GetValidMoves(board, board.King, fromSquare).Contains(square.Location))
            //    {
            //        isMovePerformed = moveManager.MakeMove(board, fromSquare, square);

            //        game.FromSquare = null;
            //    }
            //    else
            //        game.FromSquare = null;
            //}
            //else
            //    game.FromSquare = null;
            #endregion
        }
    }
}
