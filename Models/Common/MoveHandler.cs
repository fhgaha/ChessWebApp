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

        public MoveHandler(Board _board)
        {
            board = _board;
        }

        public static bool PerformMove(Board board, Square square)
        {
            bool isMovePerformed = false;

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
                if (!SquareIsEmpty() && !MoveOrderIsWrong(square))
                {
                    FromSquare = square;

                    if (FromSquare.CurrentPiece != null && !MoveOrderIsWrong(FromSquare))
                        board.UpdateValidSquares(FromSquare.CurrentPiece);
                }
            }
            else if (MoveOrderIsWrong(FromSquare))
            {
                FromSquare = null;
            }
            else if (!SquareIsEmpty() && SelectedAndTargetPieceAreTheSamePiece())
            {
                FromSquare = null;
            }
            else if (!SquareIsEmpty() && SelectedAndTargetPieceAreTheSameColor())
            {
                FromSquare = square;
                board.UpdateValidSquares(FromSquare.CurrentPiece);
            }
            else
            {   //making a move

                //board.UpdateValidSquares(FromSquare.CurrentPiece);

                if (board.ValidMoves.Contains(square.Location))
                {
                    FromSquare.CurrentPiece.MovePiece(square);
                    UpdateSquaresAttackedByPiece(board, square.CurrentPiece);

                    IsWhitesMove = !IsWhitesMove;
                    isMovePerformed = true;
                }
                FromSquare = null;
            }

            return isMovePerformed;
        }

        public static List<Location> GetMovesToPreventCheck(Board board, Square defender)
        {
            var moveCandidates = defender.CurrentPiece.GetValidMoves(board, defender);
            King king = IsWhitesMove ? board.WhiteKing : board.BlackKing;
            var attackers = new List<AbstractPiece>(king.CurrentSquare.AttackedByPieces);
            List<Location> moves = new List<Location>();

            foreach (var loc in moveCandidates)
            {
                var candidateSquare = board.LocationSquareMap[loc];

                MakeFakeMove(defender, candidateSquare, attackers, () =>
                {
                    if (!king.IsUnderCheck()) moves.Add(loc);
                    return;
                });
            }

            board.SetAllSquaresNotValid();
            moves.ForEach(loc => board.LocationSquareMap[loc].IsValid = true);

            return moves;
        }

        private static void MakeFakeMove(Square from, Square to, List<AbstractPiece> attackers, Action action)
        {
            AbstractPiece savedFromPiece = from.CurrentPiece;

            AbstractPiece savedToPiece = null;
            if (to.CurrentPiece != null)
                savedToPiece = to.CurrentPiece;

            from.CurrentPiece.MovePiece(to);
            attackers.ForEach(p => UpdateSquaresAttackedByPiece(board, p));
            if (to.CurrentPiece.isFirstMove == true) to.CurrentPiece.isFirstMove = true;

            action();

            to.CurrentPiece.MovePiece(from);
            attackers.ForEach(p => UpdateSquaresAttackedByPiece(board, p));
            if (from.CurrentPiece.isFirstMove == true) from.CurrentPiece.isFirstMove = true;

            if (savedToPiece != null)
            {
                to.CurrentPiece = savedToPiece;
                to.IsOccupied = true;
            }
        }

        private static void UpdateSquaresAttackedByPiece(Board board, AbstractPiece attacker)
        {
            foreach (Square sq in board.LocationSquareMap.Values)
                if (sq.AttackedByPieces.Contains(attacker))
                    sq.AttackedByPieces.Remove(attacker);

            var attackedLocs = attacker.GetLocationsAttackedByPiece(board);

            attackedLocs.ForEach(loc => board.LocationSquareMap[loc].AttackedByPieces.Add(attacker));
        }
    }
}
