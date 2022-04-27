using ChessWebApp.Models.Notation;
using ChessWebApp.Models.Players;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChessWebApp.Models
{
    public class Game
    {
        public Player PlayerWhite;
        public Player PlayerBlack;
        public Player PlayerToMove;

        public bool IsOver = false;
        public Board Board;
        public MoveManager MoveManager;
        public Fen Fen;

        public string message = "";
        
        private Square fromSquare;
        public Square FromSquare
        {
            get => fromSquare;
            set
            {
                if (value is null)
                {
                    MoveManager.ClearValidMoves();
                    if (fromSquare is not null) fromSquare.IsSelected = false;
                    Board.SetAllSquaresNotValid();
                    fromSquare = value;
                    return;
                }

                if (fromSquare is not null) fromSquare.IsSelected = false;
                Board.SetAllSquaresNotValid();
                value.IsSelected = true;
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

            Fen = new();
            Board = new(pieces);
            MoveManager = new();
            PlayerWhite = new HumanPlayer();
            PlayerBlack = new HumanPlayer();
            PlayerToMove = PlayerWhite;
        }

        public Game(string fenCode)
        {
            Fen = new();
            Board = Fen.Parse(fenCode);
            MoveManager = new();
            PlayerWhite = new HumanPlayer();
            PlayerBlack = new HumanPlayer();
            PlayerToMove = PlayerWhite;
        }

        public void HandleClick(Square square)
        {
            bool isMovePerformed = PlayerToMove.MakeMove(this, MoveManager, square);

            if (isMovePerformed)
                PlayerToMove = PlayerToMove == PlayerWhite ? PlayerBlack : PlayerWhite;

            Board.WhiteKing.UpdateIsUnderCheck(Board.LocationSquareMap[Board.WhiteKing.Location]);
            Board.BlackKing.UpdateIsUnderCheck(Board.LocationSquareMap[Board.BlackKing.Location]);
        }

        //private bool HandleClickAsHuman(Square square, bool isMovePerformed)
        //{
        //    Func<bool> FromSquareIsSelected = () => FromSquare != null && FromSquare.CurrentPiece != null;
        //    Func<bool> SquareIsEmpty = () => square.CurrentPiece == null;

        //    Func<bool> SelectedAndTargetPieceAreTheSamePiece = ()
        //        => FromSquare == square && FromSquare.CurrentPiece != null;

        //    Func<bool> SelectedAndTargetPieceAreTheSameColor = ()
        //        => FromSquare.CurrentPiece.PieceColor == square.CurrentPiece.PieceColor;

        //    Func<Square, bool> MoveOrderIsWrong = sq
        //         => Board.IsWhitesMove && sq.CurrentPiece.PieceColor != PieceColor.Light
        //        || !Board.IsWhitesMove && sq.CurrentPiece.PieceColor != PieceColor.Dark;


        //    if (!FromSquareIsSelected())
        //    {
        //        if (SquareIsEmpty())
        //            FromSquare = null;
        //        else if (MoveOrderIsWrong(square))
        //            FromSquare = null;
        //        else
        //        {
        //            FromSquare = square;
        //            MoveManager.UpdateValidSquares(Board, Board.King, square);
        //        }
        //    }
        //    else if (FromSquareIsSelected())
        //    {
        //        if (MoveOrderIsWrong(FromSquare))
        //            FromSquare = null;
        //        else if (!SquareIsEmpty() && SelectedAndTargetPieceAreTheSamePiece())
        //            FromSquare = null;
        //        else if (!SquareIsEmpty() && SelectedAndTargetPieceAreTheSameColor())
        //        {
        //            FromSquare = square;
        //            MoveManager.UpdateValidSquares(Board, Board.King, FromSquare);
        //        }
        //        else if (MoveManager.GetValidMovesToDisplay().Contains(square.Location))  //selected and moving allowed
        //        {
        //            isMovePerformed = MoveManager.MakeMove(Board, FromSquare, square);
        //            Board.IsWhitesMove = !Board.IsWhitesMove;
        //            PlayerToMove = PlayerToMove == PlayerWhite ? PlayerBlack : PlayerWhite;
        //            FromSquare = null;
        //        }
        //        else
        //            FromSquare = null;
        //    }
        //    else
        //        FromSquare = null;
        //    return isMovePerformed;
        //}

        public void PromotePawn(AbstractPiece piece) => MoveManager.PromotePawn(Board, piece);

        public List<Location> GetValidMoves() => MoveManager.GetValidMovesToDisplay();
    }
}
