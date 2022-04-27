using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChessWebApp.Models.Players
{
    public interface Player
    {
        public int Score { get; set; }
        public List<AbstractPiece> CapturedPieces { get; set; }

        bool MakeMove(Game game, MoveManager moveManager, Square square);
    }

    public class HumanPlayer : Player
    {
        public int Score { get; set; }
        public List<AbstractPiece> CapturedPieces { get; set; }
        public bool MakeMove(Game game, MoveManager moveManager, Square square)
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
                    moveManager.UpdateValidSquares(board, board.King, square);
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
                    moveManager.UpdateValidSquares(board, board.King, fromSquare);
                }
                else if (moveManager.GetValidMovesToDisplay().Contains(square.Location))  //selected and moving allowed
                {
                    isMovePerformed = moveManager.MakeMove(board, fromSquare, square);
                    board.IsWhitesMove = !board.IsWhitesMove;

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

    public class MachinePlayer : Player
    {
        public int Score { get; set; }
        public List<AbstractPiece> CapturedPieces { get; set; }
        public bool MakeMove(Game game, MoveManager moveManager, Square square)
        {
            return false;
        }
    }
}
