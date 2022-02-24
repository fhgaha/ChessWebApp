using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessWebApp
{
    public class Queen : AbstractPiece
    {
        IMovable bishop, rook;

        public Queen(PieceColor pieceColor) : base(pieceColor)
        {
            Name = "Queen";
        }

        public Queen(PieceColor pieceColor, IMovable bishop, IMovable rook) : this(pieceColor)
        {
            this.bishop = bishop;
            this.rook = rook;
        }

        public override List<Location> GetValidMoves(Board board, Square square)
        {
            return GetValidMoves(board);
        }

        public override List<Location> GetValidMoves(Board board)
        {
            var moveCandidates = new List<Location>();
            moveCandidates.AddRange(bishop.GetValidMoves(board, this.CurrentSquare));
            moveCandidates.AddRange(rook.GetValidMoves(board, this.CurrentSquare));
            return moveCandidates;
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
