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
            this.bishop = new Bishop(PieceColor);
            this.rook = new Rook(PieceColor);
            Name = "Queen";
        }

        public Queen(Queen piece) : this(piece.PieceColor) { }

        public override List<Location> GetLocationsAttackedByPiece(Board board, Square attacker)
        {
            return GetValidMoves(board, attacker);
        }

        public override List<Location> GetValidMoves(Board board, Square from)
        {
            var moveCandidates = new List<Location>();
            moveCandidates.AddRange(bishop.GetValidMoves(board, from));
            moveCandidates.AddRange(rook.GetValidMoves(board, from));

            moveCandidates.ForEach(loc => board.LocationSquareMap[loc].IsValid = true);

            return moveCandidates;
        }
    }
}
