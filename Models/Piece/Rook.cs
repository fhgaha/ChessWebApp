using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessWebApp
{
    public class Rook : AbstractPiece
    {
        public bool IsFirstMove = true;

        public Rook(PieceColor pieceColor) : base(pieceColor)
        {
            Name = "Rook";
        }

        public override List<Location> GetValidMoves(Board board, Square square)
        {
            CurrentSquare = square;
            return GetValidMoves(board);
        }

        public override List<Location> GetValidMoves(Board board)
        {
            var moveCandidates = new List<Location>();
            var squareMap = board.LocationSquareMap;
            var current = CurrentSquare.Location;
            GetFileCandidates(moveCandidates, squareMap, current, -1);
            GetFileCandidates(moveCandidates, squareMap, current, 1);
            GetRankCandidates(moveCandidates, squareMap, current, -1);
            GetRankCandidates(moveCandidates, squareMap, current, 1);

            //need castle logic

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

        public override void MakeMove(Square square)
        {
            IsFirstMove = false;
            square.IsOccupied = true;
            square.CurrentPiece = this;
            CurrentSquare.Reset();
            CurrentSquare = square;
        }

        public override List<Location> GetLocationsAttackedByPiece()
        {
            throw new NotImplementedException();
        }
    }
}
