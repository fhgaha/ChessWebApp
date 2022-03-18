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

            if (IsCastlingPossible(board, from))
                moveCandidates.AddRange(GetCastlingMoves());

            board.SetAllSquaresNotValid();
            moveCandidates.ForEach(loc => board.LocationSquareMap[loc].IsValid = true);
            return moveCandidates;
        }

        private bool IsCastlingPossible(Board board, Square from)
        {
            if (!isFirstMove) return false;

            if (PieceColor == PieceColor.Light && (IsAbleToCastleLeft(board) || IsAbleToCastleRight(board)))
                return true;
            else if (PieceColor == PieceColor.Dark && (IsAbleToCastleLeft(board) || IsAbleToCastleRight(board)))
                return true;

            return false;
        }

        private bool IsAbleToCastleRight(Board board)
        {
            List<Square> squaresInQuestion = new();
            squaresInQuestion.AddRange(
                PieceColor == PieceColor.Light ?
                new List<Square>
                {
                    board.LocationSquareMap[new Location(File.F, 1)],
                    board.LocationSquareMap[new Location(File.G, 1)],
                    board.LocationSquareMap[new Location(File.H, 1)]
                }
                : new List<Square>
                {
                    board.LocationSquareMap[new Location(File.F, 8)],
                    board.LocationSquareMap[new Location(File.G, 8)],
                    board.LocationSquareMap[new Location(File.H, 8)]
                });

        }

        private bool IsAbleToCastleLeft()
        {
            throw new NotImplementedException();
        }

        private IEnumerable<Location> GetCastlingMoves()
        {
            throw new NotImplementedException();
        }

    }
}
