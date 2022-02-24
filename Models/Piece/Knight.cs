using System.Collections.Generic;

namespace ChessWebApp
{
    public class Knight : AbstractPiece
    {
        public Knight(PieceColor pieceColor) : base(pieceColor)
        {
            Name = "Knight";
        }

        public List<Location> GetValidMoves(Board board)
        {
            var moveCandidates = new List<Location>();
            var squareMap = board.LocationSquareMap;
            var current = CurrentSquare.Location;
            GetMoves(moveCandidates, squareMap, current, 2, 1);
            GetMoves(moveCandidates, squareMap, current, -2, 1);
            GetMoves(moveCandidates, squareMap, current, 2, -1);
            GetMoves(moveCandidates, squareMap, current, -2, -1);

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

            if (squareMap.ContainsKey(next))
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

        public List<Location> GetValidMoves(Board board, Square square)
        {
            throw new System.NotImplementedException();
        }

        public override void MakeMove(Square square)
        {
            square.IsOccupied = true;
            square.CurrentPiece = this;
            CurrentSquare.Reset();
            CurrentSquare = square;
        }
    }
}
