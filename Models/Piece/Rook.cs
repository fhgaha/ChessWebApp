using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessWebApp
{
    public class Rook : AbstractPiece
    {
        public Rook(PieceColor pieceColor) : base(pieceColor)
        {
            Name = "Rook";
        }

        public Rook(Rook piece) : this(piece.PieceColor) { }

        public override List<Location> GetValidMoves(Board board, Square from)
        {
            List<Location> moveCandidates = new();
            GetFileCandidates(moveCandidates, board.LocationSquareMap, from.Location, -1);
            GetFileCandidates(moveCandidates, board.LocationSquareMap, from.Location, 1);
            GetRankCandidates(moveCandidates, board.LocationSquareMap, from.Location, -1);
            GetRankCandidates(moveCandidates, board.LocationSquareMap, from.Location, 1);

            //need castle logic

            moveCandidates.ForEach(loc => board.LocationSquareMap[loc].IsValid = true);

            return moveCandidates;
        }

        private void GetRankCandidates(
            List<Location> moveCandidates,
            Dictionary<Location, Square> squareMap,
            Location current,
            int offset)
        {
            var next = LocationFactory.Build(current, 0, offset);

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
                next = LocationFactory.Build(next, 0, offset);
            }
        }

        private void GetFileCandidates(
            List<Location> moveCandidates,
            Dictionary<Location, Square> squareMap,
            Location current,
            int offset)
        {
            var next = LocationFactory.Build(current, offset, 0);

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
                next = LocationFactory.Build(next, offset, 0);
            }
        }

        public override List<Location> GetLocationsAttackedByPiece(Board board, Square attacker)
        {
            return GetValidMoves(board, attacker);
        }
    }
}
