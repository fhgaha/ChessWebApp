using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessWebApp
{
    public class King : AbstractPiece
    {
        public bool IsUnderCheck(Square kingSquare)
        {
            if (kingSquare.AttackedByPiecesOnSquares
                .Any(attackerSquare => attackerSquare.CurrentPiece.PieceColor != PieceColor))
                return true;
            return false;
        }

        public King(PieceColor pieceColor) : base(pieceColor)
        {
            Name = "King";
        }

        public King(King piece) : this(piece.PieceColor) { }

        public override List<Location> GetLocationsAttackedByPiece(Board board, Square attacker)
        {
            var attackedLocations = new List<Location>();

            //get neighbour squares
            for (int i = 0; i < 9; i++)
            {
                int rankOffset = i % 3 - 1;
                int fileOffset = i / 3 - 1;

                if (rankOffset == 0 && fileOffset == 0) continue;

                attackedLocations.Add(LocationFactory.Build(attacker.Location, rankOffset, fileOffset));
            }

            return attackedLocations.Where(loc => loc != null).ToList();
        }

        public override List<Location> GetValidMoves(Board board, Square from)
        {
            var moveCandidates = GetLocationsAttackedByPiece(board, from)
                .Where(loc =>
                {
                    var square = board.LocationSquareMap[loc];
                    if (square.AttackedByPiecesOnSquares.Any(ap => ap.CurrentPiece.PieceColor != PieceColor))
                        return false;
                    return true;
                })
                .Where(candidate =>
                    Math.Abs((int)candidate.File - (int)from.Location.File) < 2
                    && Math.Abs(candidate.Rank - from.Location.Rank) < 2
                    &&
                    (
                        board.LocationSquareMap[candidate].IsOccupied == false
                        || board.LocationSquareMap[candidate].IsOccupied
                            && board.LocationSquareMap[candidate].CurrentPiece.PieceColor != PieceColor
                        || (board.LocationSquareMap[candidate].CurrentPiece.PieceColor != PieceColor
                            && board.LocationSquareMap[candidate].CurrentPiece is not King)
                    ))
                .ToList();

            board.SetAllSquaresNotValid();
            moveCandidates.ForEach(loc => board.LocationSquareMap[loc].IsValid = true);

            return moveCandidates;
        }
    }
}
