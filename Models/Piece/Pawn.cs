using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessWebApp
{
    public class Pawn : AbstractPiece
    {
        public Pawn(PieceColor pieceColor) : base(pieceColor)
        {
            Name = "Pawn";
        }

        public Pawn(Pawn piece) : this(piece.PieceColor) { }

        public override List<Location> GetValidMoves(Board board, Square start)
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

            validLocations.ForEach(loc => squareMap[loc].IsValid = true);

            return validLocations;
        }

        private void TryAddEnPassantMove(Board board, List<Location> validLocations)
        {
            //Взятие пешки противника может осуществляться только сразу после её перемещения на два поля
            var prevMoveFromSquare = board.PerformedMoves.LastOrDefault()?.Item1;
            var prevMoveToSquare = board.PerformedMoves.LastOrDefault()?.Item2;

            if (prevMoveToSquare?.CurrentPiece is Pawn pawn && pawn.PieceColor != PieceColor
                && Math.Abs(prevMoveFromSquare.Location.Rank - prevMoveToSquare.Location.Rank) == 2)
            {
                if (LocationFactory.Build(Location, 1, 0) is Location loc && pawn.Location == loc)
                {
                    validLocations.Add(LocationFactory.Build(loc, 0, PieceColor == PieceColor.Light ? 1 : -1));
                    board.PawnToBeTakenEnPassant = pawn;
                }

                else if (LocationFactory.Build(Location, -1, 0) is Location otherLoc && pawn.Location == otherLoc)
                {
                    validLocations.Add(LocationFactory.Build(otherLoc, 0, PieceColor == PieceColor.Light ? 1 : -1));
                    board.PawnToBeTakenEnPassant = pawn;
                }
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
