using System.Collections.Generic;

namespace ChessWebApp
{
    public class Knight : AbstractPiece
    {
        public Knight(PieceColor pieceColor) : base(pieceColor)
        {
            Name = "Knight";
        }

        public Knight(Knight piece) : this(piece.PieceColor) { }

        public override List<Location> GetMoves(Board board, Square from)
        {
            var moveCandidates = new List<Location>();
            GetMoves(moveCandidates, board.LocationSquareMap, from.Location, 2, 1);
            GetMoves(moveCandidates, board.LocationSquareMap, from.Location, 2, -1);
            GetMoves(moveCandidates, board.LocationSquareMap, from.Location, -2, 1);
            GetMoves(moveCandidates, board.LocationSquareMap, from.Location, -2, -1);

            GetMoves(moveCandidates, board.LocationSquareMap, from.Location, 1, 2);
            GetMoves(moveCandidates, board.LocationSquareMap, from.Location, -1, 2);
            GetMoves(moveCandidates, board.LocationSquareMap, from.Location, 1, -2);
            GetMoves(moveCandidates, board.LocationSquareMap, from.Location, -1, -2);

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

            if (next != null)   //(squareMap.ContainsKey(next))
            {
                if (squareMap[next].IsOccupied)
                {
                    if (squareMap[next].CurrentPiece.PieceColor == PieceColor)
                        return;
                    moveCandidates.Add(next);
                    return;
                }
                moveCandidates.Add(next);
            }
        }

        public override List<Location> GetLocationsAttackedByPiece(Board board, Square attacker)
        {
            return GetMoves(board, attacker);
        }
    }
}
