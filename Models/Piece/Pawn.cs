using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessWebApp
{
    public class Pawn : AbstractPiece
    {
        public bool IsReadyToPromote { get; set; } = false;

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
            if (isFirstMove) moveCandidates.Add(LocationFactory.Build(start.Location, 0, rankOffset * 2));

            //need en passant logic

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

            validLocations.ForEach(loc => squareMap[loc].IsValid = true);

            return validLocations;
        }

        public override List<Location> GetLocationsAttackedByPiece(Board board, Square attacker)
        {
            List<Location> attackedLocations = new List<Location>();

            var rankOffset = PieceColor == PieceColor.Light ? 1 : -1;

            attackedLocations.Add(LocationFactory.Build(attacker.Location, -1, rankOffset));
            attackedLocations.Add(LocationFactory.Build(attacker.Location, 1, rankOffset));

            return attackedLocations.Where(l => l != null).ToList();
        }

        //public void UpdateValuesAfterMove()
        //{
        //    if (PieceColor == PieceColor.Light && Location.Rank == 8
        //        || PieceColor == PieceColor.Dark && Location.Rank == 1)
        //    {
        //        IsReadyToPromote = true;
        //    }
        //}
    }
}
