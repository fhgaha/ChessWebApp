using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessWebApp
{
    public class King : AbstractPiece
    {
        IMovable bishop, rook;

        public bool IsUnderCheck()
        {
            if (CurrentSquare.AttackedByPieces.Any(piece => piece.PieceColor != PieceColor))
                return true;
            return false;
        }

        public King(PieceColor pieceColor) : base(pieceColor)
        {
            bishop = new Bishop(pieceColor);
            rook = new Rook(pieceColor);
            Name = "King";
        }

        public override List<Location> GetLocationsAttackedByPiece(Board board)
        {
            var attackedLocations = new List<Location>();

            //get neighbour squares
            for (int i = 0; i < 9; i++)
            {
                int rankOffset = i % 3 - 1;
                int fileOffset = i / 3 - 1;

                if (rankOffset == 0 && fileOffset == 0) continue;

                attackedLocations.Add(LocationFactory.Build(CurrentSquare.Location, rankOffset, fileOffset));
            }

            return attackedLocations.Where(loc => loc != null).ToList();
        }

        public override List<Location> GetValidMoves(Board board, Square from)
        {
            var moveCandidates = GetValidMoves(board)
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

        public override List<Location> GetValidMoves(Board board)
        {
            var locs = GetLocationsAttackedByPiece(board);
            return locs.Where(loc =>
            {
                var square = board.LocationSquareMap[loc];
                if (square.AttackedByPieces.Any(ap => ap.PieceColor != PieceColor))
                    return false;
                return true;
            }).ToList();
        }

        public override void MovePiece(Square square)
        {
            isFirstMove = false;
            square.IsOccupied = true;
            square.CurrentPiece = this;
            CurrentSquare.Reset();
            CurrentSquare = square;
        }
    }
}
