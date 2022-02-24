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
            Name = "King";
        }

        public King(PieceColor pieceColor, IMovable bishop, IMovable rook) : this(pieceColor)
        {
            this.bishop = bishop;
            this.rook = rook;
        }

        public List<Location> GetValidMoves(Board board)
        {
            var moveCandidates = new List<Location>();
            moveCandidates.AddRange(bishop.GetValidMoves(board, CurrentSquare));
            moveCandidates.AddRange(rook.GetValidMoves(board, CurrentSquare));
            var current = CurrentSquare.Location;

            //need castle logic

            return moveCandidates.Where(candidate => 
                Math.Abs((int)candidate.File - (int)current.File) == 1 &&
                Math.Abs(candidate.Rank - current.Rank) == 1
            ).ToList();
        }

        public List<Location> GetValidMoves(Board board, Square square)
        {
            return null;
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
