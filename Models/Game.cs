using ChessWebApp.Models.Notation;
using ChessWebApp.Models.Players;
using System.Collections.Generic;
using System.Linq;

public enum GameState
{
    ReadyForInput,
    NotReadyForInput,
    Finished
}

namespace ChessWebApp.Models
{
    public class Game
    {
        private GameState state;
        public Player PlayerWhite;
        public Player PlayerBlack;
        public Player PlayerToMove;
        public Board Board { get; set; }
        public MoveManager MoveManager { get; set; }
        public Fen Fen { get; set; }

        public string message = "";

        private Square fromSquare;
        public Square FromSquare
        {
            get => fromSquare;
            set
            {
                if (fromSquare is not null) fromSquare.IsSelected = false;
                fromSquare = value;
                if (value is null) return;
                value.IsSelected = true;
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
            SetUp();
        }

        public Game(string fenCode)
        {
            Fen = new();
            Board = Fen.Parse(fenCode);
            SetUp();
        }

        private void SetUp()
        {
            MoveManager = new();
            PlayerWhite = new MachinePlayer();
            PlayerBlack = new MachinePlayer();
            PlayerToMove = PlayerWhite;
        }

        public void HandleClick(Square square)
        {
            //square is null in case of machine making a move
            if (state == GameState.NotReadyForInput) return;

            state = GameState.NotReadyForInput;
            bool isMovePerformed = PlayerToMove.TryMakeMove(this, MoveManager, square);

            if (isMovePerformed)
                PlayerToMove = PlayerToMove == PlayerWhite ? PlayerBlack : PlayerWhite;

            Board.WhiteKing.UpdateIsUnderCheck(Board.LocationSquareMap[Board.WhiteKing.Location]);
            Board.BlackKing.UpdateIsUnderCheck(Board.LocationSquareMap[Board.BlackKing.Location]);

            state = GameState.ReadyForInput;
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

        public void SetAllSquaresNotValid()
            => Board.LocationSquareMap.Values.ToList().ForEach(sq => sq.IsValid = false);

        public void SetProperSquaresAsValid()
        {
            if (FromSquare is not null)
            {
                var moves = MoveManager.GetValidMoves(Board, Board.King, FromSquare);
                moves.ForEach(loc => Board.LocationSquareMap[loc].IsValid = true);
            }
        }

        public List<Location> GetValidMoves()
        {
            if (FromSquare is not null)
                return MoveManager.GetValidMoves(Board, Board.King, FromSquare);

            return new List<Location>();
        }

        public void UndoMove()
        {
            MoveManager.UndoMove(Board);

            Board.WhiteKing.UpdateIsUnderCheck(Board.LocationSquareMap[Board.WhiteKing.Location]);
            Board.BlackKing.UpdateIsUnderCheck(Board.LocationSquareMap[Board.BlackKing.Location]);
        }
    }
}
