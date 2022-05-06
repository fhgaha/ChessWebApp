using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessWebApp
{
    public class Pawn : AbstractPiece
    {
        public bool IsPromotingNextMove
        {
            get
            {
                return PieceColor == PieceColor.Light
                    ? Location.Rank == 7 ? true : false
                    : Location.Rank == 2 ? true : false;
            }
        }
        public Pawn(PieceColor pieceColor) : base(pieceColor)
        {
            Name = "Pawn";
        }

        public Pawn(Pawn piece) : this(piece.PieceColor) { }

        public override List<Location> GetMoves(Board board, Square start)
        {
            var moveCandidates = new List<Location>();
            var squareMap = board.LocationSquareMap;

            var rankOffset = PieceColor == PieceColor.Light ? 1 : -1;

            moveCandidates.Add(LocationFactory.Build(start.Location, 0, rankOffset));
            moveCandidates.Add(LocationFactory.Build(start.Location, 1, rankOffset));
            moveCandidates.Add(LocationFactory.Build(start.Location, -1, rankOffset));
            if (IsFirstMove) moveCandidates.Add(LocationFactory.Build(start.Location, 0, rankOffset * 2));

            var validLocations = moveCandidates.Where(c => c != null).Where(candidate =>
            {
                if (!squareMap.ContainsKey(candidate))
                    return false;
                else if (candidate.File == start.Location.File)
                {
                    if (squareMap[candidate].IsOccupied)
                        return false;
                }
                else if (squareMap[candidate].CurrentPiece == null)
                    return false;
                else if (squareMap[candidate].CurrentPiece.PieceColor == PieceColor)
                    return false;

                return true;
            }).ToList();

            TryAddEnPassantMove(board, validLocations);

            //validLocations.ForEach(loc => squareMap[loc].IsValid = true);

            return validLocations;
        }

        private void TryAddEnPassantMove(Board board, List<Location> validLocations)
        {
            //Взятие пешки противника может осуществляться только сразу после её перемещения на два поля

            if (board.PawnToBeTakenEnPassant is null) return;

            Location locToRight = LocationFactory.Build(Location, 1, 0);
            Location locToLeft = LocationFactory.Build(Location, -1, 0);
            AbstractPiece pieceToRight = null;
            AbstractPiece pieceToLeft = null; 
            if (locToRight is not null) pieceToRight = board.LocationSquareMap[locToRight].CurrentPiece;
            if (locToLeft is not null) pieceToLeft = board.LocationSquareMap[locToLeft].CurrentPiece;

            if (board.PawnToBeTakenEnPassant == pieceToRight
                || board.PawnToBeTakenEnPassant == pieceToLeft)
            {
                Location locToMoveTo = LocationFactory.Build(
                    board.PawnToBeTakenEnPassant.Location, 0, PieceColor == PieceColor.Light ? 1 : -1);
                validLocations.Add(locToMoveTo);
            }
        }

        public override List<Location> GetLocationsAttackedByPiece(Board board, Square attacker)
        {
            List<Location> attackedLocations = new List<Location>();

            var rankOffset = PieceColor == PieceColor.Light ? 1 : -1;

            attackedLocations.Add(LocationFactory.Build(attacker.Location, -1, rankOffset));
            attackedLocations.Add(LocationFactory.Build(attacker.Location, 1, rankOffset));

            return attackedLocations.Where(l => l != null).ToList();
        }
    }
}
