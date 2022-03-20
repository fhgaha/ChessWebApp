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
        public List<Tuple<Square, Square>> PerformedMoves = new List<Tuple<Square, Square>>();
        public List<Location> ValidMoves { get; private set; } = new();
        public string message = "";
        public bool IsWhitesMove = true;
        private Square fromSquare;
        private Square FromSquare
        {
            get => fromSquare;
            set
            {
                if (value == null) ValidMoves.Clear();
                Board.LocationSquareMap.Values.ToList().ForEach(sq => sq.IsValid = false);
                fromSquare = value;
            }
        }

        public Game()
        {
            Board = new();
        }

        public bool HandleClick(Square square)
        {
            bool isMovePerformed = true;

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
                    UpdateValidSquares(square);
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
                    UpdateValidSquares(FromSquare);
                }
                else if (ValidMoves.Contains(square.Location))  //selected and moving allowed
                {
                    isMovePerformed = MakeMove(Board, square);
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


        private bool MakeMove(Board board, Square square)
        {
            if (square.CurrentPiece != null) RemoveAttackerFromAllAttackedByPieceOnSquareLists(Board, square);
            RemoveAttackerFromAllAttackedByPieceOnSquareLists(Board, FromSquare);

            FromSquare.MovePiece(square);

            //castling
            if (square.CurrentPiece is King && Math.Abs(FromSquare.Location.File - square.Location.File) == 2)
                HandleCastling(square);

            board.LocationSquareMap.Values.ToList().ForEach(sq => UpdateSquaresAttackedByPiece(Board, sq));

            //MovingPieceBesidesTheseLocsDiscoversCheck(square).ForEach(loc => board.LocationSquareMap[loc].IsValid = true);

            IsWhitesMove = !IsWhitesMove;

            board.PerformedMoves.Add(Tuple.Create(fromSquare, square));
            return true;
        }

        public void HandleCastling(Square square)
        {
            Square FromRookSquare = null;
            Square ToRookSquare = null;
            if (square.Location == new Location(File.C, 1))
            {
                if (Board.LocationSquareMap[new Location(File.D, 1)].IsOccupied) return;

                FromRookSquare = Board.LocationSquareMap[new Location(File.A, 1)];
                ToRookSquare = Board.LocationSquareMap[new Location(File.D, 1)];
            }
            else if (square.Location == new Location(File.G, 1))
            {
                FromRookSquare = Board.LocationSquareMap[new Location(File.H, 1)];
                ToRookSquare = Board.LocationSquareMap[new Location(File.F, 1)];
            }
            else if (square.Location == new Location(File.C, 8))
            {
                FromRookSquare = Board.LocationSquareMap[new Location(File.A, 8)];
                ToRookSquare = Board.LocationSquareMap[new Location(File.D, 8)];
            }
            else if (square.Location == new Location(File.G, 8))
            {
                FromRookSquare = Board.LocationSquareMap[new Location(File.H, 8)];
                ToRookSquare = Board.LocationSquareMap[new Location(File.F, 8)];
            }

            RemoveAttackerFromAllAttackedByPieceOnSquareLists(Board, FromRookSquare);
            FromRookSquare.MovePiece(ToRookSquare);
            UpdateSquaresAttackedByPiece(Board, ToRookSquare);
        }

        public void UpdateValidSquares(Square square)
        {
            var legalMoves = MovingPieceBesidesTheseLocsDiscoversCheck(square);

            Board.SetAllSquaresNotValid();
            legalMoves.ForEach(loc => Board.LocationSquareMap[loc].IsValid = true);
            ValidMoves = legalMoves;
        }

        private List<Location> MovingPieceBesidesTheseLocsDiscoversCheck(Square defender)
        {
            var moves = defender.CurrentPiece.GetValidMoves(Board, defender);
            King king = IsWhitesMove ? Board.WhiteKing : Board.BlackKing;

            moves = FilterMovesToPreventCheck(Board, moves, defender.CurrentPiece, king);

            return moves;
        }

        public List<Location> FilterMovesToPreventCheck(Board originalBoard, List<Location> candidates,
            AbstractPiece defender, King king)
        {
            List<Location> defendingMoves = new List<Location>();

            foreach (Location candidate in candidates)
            {
                Board fBoard = new Board() { IsReal = false };

                originalBoard.LocationSquareMap.Keys.ToList()
                    .ForEach(loc => fBoard.LocationSquareMap[loc] = new Square(originalBoard.LocationSquareMap[loc]));

                //copy attacked by pieces on squares list for each square
                foreach (Location loc in fBoard.LocationSquareMap.Keys)
                {
                    var originalSquareAttackersList = originalBoard.LocationSquareMap[loc].AttackedByPiecesOnSquares;
                    var fakeSquareAttackersList = fBoard.LocationSquareMap[loc].AttackedByPiecesOnSquares;

                    fBoard.LocationSquareMap[loc].CopyAttackedByPiecesOnSquares(fBoard, originalSquareAttackersList);
                }

                Square squareCandidate = fBoard.LocationSquareMap[candidate];
                Square fDefenderSquare = fBoard.LocationSquareMap[defender.Location];
                Square fKingSquare = fBoard.LocationSquareMap[king.Location];
                var originalkingAttackersList = originalBoard.LocationSquareMap[king.Location].AttackedByPiecesOnSquares;
                fKingSquare.CopyAttackedByPiecesOnSquares(fBoard, originalkingAttackersList);

                MakeFakeMove(fDefenderSquare, squareCandidate, defendingMoves, candidate, fBoard,
                    (King)fKingSquare.CurrentPiece);
            }
            return defendingMoves;
        }

        private void MakeFakeMove(Square from, Square to, List<Location> defendingMoves,
            Location candidate, Board fBoard, King fKing)
        {
            //if candidate contains attacker, after moving a defender the attaker would be taken.
            //update king's square for every other attacker and check if king is still attacked

            Square fKingSquare = fBoard.LocationSquareMap[fKing.Location];

            if (to.IsOccupied && fKingSquare.AttackedByPiecesOnSquares.Contains(to))
            {
                RemoveAttackerFromAllAttackedByPieceOnSquareLists(fBoard, from);
                from.MovePiece(to);
                fBoard.LocationSquareMap.Values.ToList().ForEach(sq => UpdateSquaresAttackedByPiece(fBoard, sq));

                fKingSquare.AttackedByPiecesOnSquares.Where(p => p.Location != candidate).ToList()
                    .ForEach(attacker => UpdateSquaresAttackedByPiece(fBoard, attacker));
            }
            else
            {
                RemoveAttackerFromAllAttackedByPieceOnSquareLists(fBoard, from);
                from.MovePiece(to);
                fBoard.LocationSquareMap.Values.ToList().ForEach(sq => UpdateSquaresAttackedByPiece(fBoard, sq));

                fBoard.LocationSquareMap.Values.Where(sq => sq.CurrentPiece != null)
                    .ToList()
                    .ForEach(sq => UpdateSquaresAttackedByPiece(fBoard, sq));

                List<Square> attackers = new();
                attackers.AddRange(fKingSquare.AttackedByPiecesOnSquares);
                attackers.ForEach(attacker => UpdateSquaresAttackedByPiece(fBoard, attacker));
            }

            fKingSquare = fBoard.LocationSquareMap[fKing.Location];
            if (fKing.UpdateIsUnderCheck(fKingSquare) == false)
                defendingMoves.Add(candidate);
        }

        public void UpdateSquaresAttackedByPiece(Board board, Square attacker)
        {
            if (attacker.CurrentPiece == null) return;

            RemoveAttackerFromAllAttackedByPieceOnSquareLists(board, attacker);

            var attackedLocs = attacker.CurrentPiece.GetLocationsAttackedByPiece(board, attacker);

            attackedLocs.ForEach(loc => board.LocationSquareMap[loc].AttackedByPiecesOnSquares.Add(attacker));
        }

        public void RemoveAttackerFromAllAttackedByPieceOnSquareLists(Board board, Square attacker) =>
            board.LocationSquareMap.Values
                .Where(sq => sq.AttackedByPiecesOnSquares.Contains(attacker)).ToList()
                .ForEach(sq => sq.AttackedByPiecesOnSquares.Remove(attacker));
    }
}
