using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessWebApp
{
    public class King : AbstractPiece
    {
        IMovable bishop, rook;
        private bool isFirstMove = true;

        public King(PieceColor pieceColor) : base(pieceColor)
        {
            bishop = new Bishop(pieceColor);
            rook = new Rook(pieceColor);
            Name = "King";
        }

        public override List<Location> GetLocationsAttackedByPiece(Board board)
        {
            var moveCandidates = new List<Location>();
            moveCandidates.AddRange(bishop.GetValidMoves(board, CurrentSquare));
            moveCandidates.AddRange(rook.GetValidMoves(board, CurrentSquare));
            var current = CurrentSquare.Location;

            return moveCandidates.Where(candidate =>
                Math.Abs((int)candidate.File - (int)current.File) < 2 &&
                Math.Abs(candidate.Rank - current.Rank) < 2)
                .ToList();
        }

        public override List<Location> GetValidMoves(Board board, Square square)
        {
            return GetValidMoves(board);
        }

        public override List<Location> GetValidMoves(Board board)
        {
            return GetLocationsAttackedByPiece(board).Where(loc =>
            {
                var square = board.LocationSquareMap[loc];
                if (square.AttackedByPieces.Any(ap => ap.PieceColor != PieceColor))
                    return false;
                return true;
            }).ToList();
        }

        public override void MakeMove(Square square)
        {
            isFirstMove = false;
            square.IsOccupied = true;
            square.CurrentPiece = this;
            CurrentSquare.Reset();
            CurrentSquare = square;
        }
    }
}
