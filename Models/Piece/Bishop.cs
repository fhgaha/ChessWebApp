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
            GetMoves(moveCandidates, squareMap, current, 1, 1);
            GetMoves(moveCandidates, squareMap, current, -1, 1);
            GetMoves(moveCandidates, squareMap, current, 1, -1);
            GetMoves(moveCandidates, squareMap, current, -1, -1);

            //need logic if candidate is enemy king
            //need logic that does not allow to pass another piece

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

        public override void MakeMove(Square square)
        {
            square.IsOccupied = true;
            square.CurrentPiece = this;
            CurrentSquare.Reset();
            CurrentSquare = square;
        }

        public override List<Location> GetLocationsAttackedByPiece(Board board)
        {
            return GetValidMoves(board);
        }
    }
}
