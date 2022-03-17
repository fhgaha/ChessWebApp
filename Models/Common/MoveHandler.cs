using ChessWebApp.Models.Common;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChessWebApp
{
    public class MoveHandler
    {
        public List<Tuple<Square, Square>> completedMoves = new List<Tuple<Square, Square>>();
        public static bool IsWhitesMove = true;
        private static Board board;
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

        //public MoveHandler(Board _board)
        //{
        //    board = _board;
        //}

        //public bool PerformMove(Board board, Square square)
        //{
        //    bool isMovePerformed = true;

        //    Func<bool> FromSquareIsSelected = () => FromSquare != null && FromSquare.CurrentPiece != null;
        //    Func<bool> SquareIsEmpty = () => square.CurrentPiece == null;

        //    Func<bool> SelectedAndTargetPieceAreTheSamePiece = ()
        //        => FromSquare == square && FromSquare.CurrentPiece != null;

        //    Func<bool> SelectedAndTargetPieceAreTheSameColor = ()
        //        => FromSquare.CurrentPiece.PieceColor == square.CurrentPiece.PieceColor;

        //    Func<Square, bool> MoveOrderIsWrong = sq
        //         => IsWhitesMove && sq.CurrentPiece.PieceColor != PieceColor.Light
        //        || !IsWhitesMove && sq.CurrentPiece.PieceColor != PieceColor.Dark;


        //    if (!FromSquareIsSelected())
        //    {
        //        if (!SquareIsEmpty() && !MoveOrderIsWrong(square))
        //        {
        //            FromSquare = square;

        //            if (FromSquare.CurrentPiece != null && !MoveOrderIsWrong(FromSquare))
        //                board.UpdateValidSquares(FromSquare.CurrentPiece);
        //        }
        //    }
        //    else if (MoveOrderIsWrong(FromSquare))
        //    {
        //        FromSquare = null;
        //    }
        //    else if (!SquareIsEmpty() && SelectedAndTargetPieceAreTheSamePiece())
        //    {
        //        FromSquare = null;
        //    }
        //    else if (!SquareIsEmpty() && SelectedAndTargetPieceAreTheSameColor())
        //    {
        //        FromSquare = square;
        //        board.UpdateValidSquares(FromSquare.CurrentPiece);
        //    }
        //    else
        //    {   //making a move

        //        //board.UpdateValidSquares(FromSquare.CurrentPiece);

        //        if (board.ValidMoves.Contains(square.Location))
        //        {
        //            FromSquare.CurrentPiece.MovePiece(square);
        //            UpdateSquaresAttackedByPiece(board, square.CurrentPiece);

        //            IsWhitesMove = !IsWhitesMove;
        //            isMovePerformed = true;

        //            board.PerformedMoves.Add(Tuple.Create(fromSquare, square));
        //        }
        //        FromSquare = null;
        //    }

        //    return isMovePerformed;
        //}

        //public static List<Location> FilterMovesToPreventCheck(Board board, List<Location> candidates,
        //    AbstractPiece defender, King king)
        //{
        //    List<Location> defendingMoves = new List<Location>();

        //    foreach (var candidate in candidates)
        //    {
        //        Board fBoard = new Board();

        //        foreach (var loc in board.LocationSquareMap.Keys)
        //            fBoard.LocationSquareMap[loc] = board.LocationSquareMap[loc];

        //        Square squareCandidate = fBoard.LocationSquareMap[candidate];
        //        King fKing = new King(king);
                
        //        AbstractPiece fDefender = PieceFactory.GetNewPiece(defender);
        //        fKing.CurrentSquare.AttackedByPieces = PieceFactory.GetCopies(
        //            fBoard.LocationSquareMap[fKing.CurrentSquare.Location].AttackedByPieces);

        //        MakeFakeMove(fDefender.CurrentSquare, squareCandidate, defendingMoves, candidate, fBoard, fKing);
        //        fBoard.SetAllSquaresNotValid();
        //    }
        //    return defendingMoves;
        //}

        //private static void MakeFakeMove(Square from, Square to, List<Location> defendingMoves,
        //    Location candidate, Board fBoard, King fKing)
        //{
        //    //if candidate contains attacker, after moving a defender the attaker would be taken.
        //    //update king's square for every other attacker and check if king is still attacked

        //    if (to.IsOccupied
        //        && fKing.CurrentSquare.AttackedByPieces.Contains(to.CurrentPiece))
        //    {
        //        from.CurrentPiece.MovePiece(to);

        //        fKing.CurrentSquare.AttackedByPieces.Where(p => p.CurrentSquare.Location != candidate).ToList()
        //            .ForEach(attacker => UpdateSquaresAttackedByPiece(fBoard, attacker));
        //    }
        //    else
        //    {
        //        from.CurrentPiece.MovePiece(to);

        //        fKing.CurrentSquare.AttackedByPieces
        //            .ForEach(attacker => UpdateSquaresAttackedByPiece(fBoard, attacker));
        //    }

        //    if (!fKing.IsUnderCheck())
        //        defendingMoves.Add(candidate);

        //    to.CurrentPiece.MovePiece(from);
        //}

        //private static void UpdateSquaresAttackedByPiece(Board board, AbstractPiece attacker)
        //{
        //    foreach (Square sq in board.LocationSquareMap.Values)
        //        if (sq.AttackedByPieces.Contains(attacker))
        //            sq.AttackedByPieces.Remove(attacker);
            
        //    var attackedLocs = attacker.GetLocationsAttackedByPiece(board);

        //    attackedLocs.ForEach(loc => board.LocationSquareMap[loc].AttackedByPieces.Add(attacker));
        //}
    }
}
