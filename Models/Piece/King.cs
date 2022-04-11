using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessWebApp
{
    public class King : AbstractPiece
    {
        bool isAbleToCastle = true;
        public bool IsUnderCheck = false;
        public bool UpdateIsUnderCheck(Square kingSquare)
        {
            if (kingSquare.AttackedByPiecesOnSquares
                .Any(attackerSquare => attackerSquare.CurrentPiece.PieceColor != PieceColor))
            {
                IsUnderCheck = true;
                return true;
            }

            IsUnderCheck = false;
            return false;
        }

        public King(PieceColor pieceColor) : base(pieceColor)
        {
            Name = "King";
        }

        public King(King piece) : this(piece.PieceColor) 
        {
            isAbleToCastle = piece.isAbleToCastle;
            IsUnderCheck = piece.IsUnderCheck;
        }

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

            if (isAbleToCastle)
            {
                moveCandidates.AddRange(GetCastlingMoves(board)
                    .Select(cm => board.LocationSquareMap[cm])
                    .Where(sq => sq.IsOccupied == false)
                    .Select(sq => sq.Location));
            }

            board.SetAllSquaresNotValid();
            moveCandidates.ForEach(loc => board.LocationSquareMap[loc].IsValid = true);
            return moveCandidates;
        }


        private List<Location> GetCastlingMoves(Board board)
        {
            List<Location> moves = new();

            if (IsUnderCheck) return moves;

            //check if king ever moved
            if (!isFirstMove)
            {
                isAbleToCastle = false;
                return moves;
            }

            //check if both rooks ever moved
            var rank = PieceColor == PieceColor.Light ? 1 : 8;
            var rooks = new List<AbstractPiece>
            {
                board.LocationSquareMap[new Location(File.A, rank)].CurrentPiece,
                board.LocationSquareMap[new Location(File.H, rank)].CurrentPiece,
            }
            .Where(r => r != null).ToList();

            if (rooks.Count == 2 && rooks[0].isFirstMove == false && rooks[1].isFirstMove == false)
            {
                isAbleToCastle = false;
                return moves;
            }


            moves.Add(GetCastleLeftMove(board, rank));
            moves.Add(GetCastleRightMove(board, rank));

            return moves.Where(m => m != null).ToList();
        }

        private Location GetCastleRightMove(Board board, int rank)
        {
            List<Square> candidates = new();

            if (board.LocationSquareMap[new Location(File.F, rank)].IsOccupied
                || board.LocationSquareMap[new Location(File.G, rank)].IsOccupied)
                return null;

            candidates.AddRange(new List<Square>
                {
                    board.LocationSquareMap[new Location(File.F, rank)],
                    board.LocationSquareMap[new Location(File.G, rank)],
                    board.LocationSquareMap[new Location(File.H, rank)]
                });

            return GetCastleMove("Right", candidates);
        }

        private Location GetCastleLeftMove(Board board, int rank)
        {
            List<Square> candidates = new();

            if (board.LocationSquareMap[new Location(File.B, rank)].IsOccupied
                || board.LocationSquareMap[new Location(File.C, rank)].IsOccupied
                || board.LocationSquareMap[new Location(File.D, rank)].IsOccupied)
                return null;

            candidates.AddRange(new List<Square>
                {
                    board.LocationSquareMap[new Location(File.A, rank)],
                    board.LocationSquareMap[new Location(File.C, rank)],
                    board.LocationSquareMap[new Location(File.D, rank)]
                });

            return GetCastleMove("Left", candidates);
        }

        private Location GetCastleMove(string side, List<Square> candidates)
        {
            if (side != "Right" && side != "Left")
                throw new ArgumentException(@"side != ""Right"" && side != ""Left""");

            //check if rooks have moved

            var rooks = candidates.Where(sq => sq.CurrentPiece is Rook)
                            .Where(r => r.CurrentPiece.isFirstMove).ToList();

            if (rooks.Count == 0) return null;

            //check if castle squares are under attack by opponent pieces

            File file = side == "Right" ? File.H : File.A;

            var candidatesAttackers = candidates
                .Where(sq => sq.Location != new Location(file, 1))
                .Where(sq => sq.Location != new Location(file, 8))
                .SelectMany(sq => sq.AttackedByPiecesOnSquares)
                .Where(attackerSquare => attackerSquare.CurrentPiece.PieceColor != PieceColor)
                .ToList();

            if (candidatesAttackers.Count > 0) return null;

            return side == "Right"
                    ? PieceColor == PieceColor.Light
                        ? new Location(File.G, 1)
                        : new Location(File.G, 8)
                    : PieceColor == PieceColor.Light
                        ? new Location(File.C, 1)
                        : new Location(File.C, 8);
        }
    }
}
