using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessWebApp
{
    public class Bishop : AbstractPiece
    {
        public Bishop(PieceColor pieceColor) : base(pieceColor)
        {
            Name = "Bishop";
        }

        public Bishop(Bishop piece) : this(piece.PieceColor) { }

        public override List<Location> GetMoves(Board board, Square from)
        {
            var moveCandidates = new List<Location>();
            GetMoves(moveCandidates, board.LocationSquareMap, from.Location, 1, 1);
            GetMoves(moveCandidates, board.LocationSquareMap, from.Location, -1, 1);
            GetMoves(moveCandidates, board.LocationSquareMap, from.Location, 1, -1);
            GetMoves(moveCandidates, board.LocationSquareMap, from.Location, -1, -1);

            //need logic if candidate is enemy king
            //need logic that does not allow to pass another piece

            moveCandidates.ForEach(loc => board.LocationSquareMap[loc].IsValid = true);

            return moveCandidates;
        }

        private void GetMoves(
            List<Location> moveCandidates,
            Dictionary<Location, Square> squareMap,
            Location current,
            int rankOffset,
            int fileOffset)
        {
            var next = LocationFactory.Build(current, fileOffset, rankOffset);

            while (next != null)    //(squareMap.ContainsKey(next))
            {
                if (squareMap[next].IsOccupied)
                {
                    if (squareMap[next].CurrentPiece.PieceColor == PieceColor)
                        break;
                    moveCandidates.Add(next);
                    break;
                }
                moveCandidates.Add(next);
                next = LocationFactory.Build(next, fileOffset, rankOffset);
            }
        }

        public override List<Location> GetLocationsAttackedByPiece(Board board, Square attacker)
        {
            return GetMoves(board, attacker);
        }
    }
}
