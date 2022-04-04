using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChessWebApp.Models
{
    public class Game
    {
        public bool IsOver = false;
        public Board Board;
        public MoveManager MoveManager;

        public List<Tuple<Square, Square>> PerformedMoves = new List<Tuple<Square, Square>>();
        public string message = "";
        public bool IsWhitesMove = true;
        private Square fromSquare;
        private Square FromSquare
        {
            get => fromSquare;
            set
            {
                if (value == null) MoveManager.ClearValidMoves();
                Board.SetAllSquaresNotValid();
                fromSquare = value;
            }
        }

        public Game()
        {
            var pieces =
            PieceFactory.GetStandartPiecePositions();
            //PieceFactory.GetTwoKings();
            //PieceFactory.GetCastlingSetup();
            //PieceFactory.GetPromotionSetup();
            //PieceFactory.GetEnPassantSetup();

            Board = new(pieces);
            MoveManager = new();
        }

        public bool HandleClick(Square square)
        {
            bool isMovePerformed = true;
            King king = IsWhitesMove ? Board.WhiteKing : Board.BlackKing;

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
                if (SquareIsEmpty())
                    FromSquare = null;
                else if (MoveOrderIsWrong(square))
                    FromSquare = null;
                else
                {
                    FromSquare = square;
                    MoveManager.UpdateValidSquares(Board, king, square);
                }
            }
            else if (FromSquareIsSelected())
            {
                if (MoveOrderIsWrong(FromSquare))
                    FromSquare = null;
                else if (!SquareIsEmpty() && SelectedAndTargetPieceAreTheSamePiece())
                    FromSquare = null;
                else if (!SquareIsEmpty() && SelectedAndTargetPieceAreTheSameColor())
                {
                    FromSquare = square;
                    MoveManager.UpdateValidSquares(Board, king, FromSquare);
                }
                else if (MoveManager.GetValidMoves().Contains(square.Location))  //selected and moving allowed
                {
                    isMovePerformed = MoveManager.MakeMove(Board, FromSquare, square);
                    IsWhitesMove = !IsWhitesMove;
                    FromSquare = null;
                }
                else
                    FromSquare = null;
            }
            else
                FromSquare = null;

            Board.WhiteKing.UpdateIsUnderCheck(Board.LocationSquareMap[Board.WhiteKing.Location]);
            Board.BlackKing.UpdateIsUnderCheck(Board.LocationSquareMap[Board.BlackKing.Location]);

            return isMovePerformed;
        }

        public void PromotePawn(AbstractPiece piece) => MoveManager.PromotePawn(Board, piece);

        public List<Location> GetValidMoves() => MoveManager.GetValidMoves();
    }
}
