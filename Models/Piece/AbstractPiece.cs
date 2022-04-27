using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessWebApp
{
    public abstract class AbstractPiece : IMovable
    {
        public string Name { get; protected set; }
        public PieceColor PieceColor { get; protected set; }
        public bool IsFirstMove = true;
        public Location Location { get; set; }

        protected AbstractPiece(PieceColor pieceColor)
        {
            PieceColor = pieceColor;
        }

        public override string ToString()
        {
            return "AbstractPiece{" +
                "name = " + Name +
                ", pieceColor = " + PieceColor +
                "}";
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            if (obj is AbstractPiece piece 
                && piece.Location == Location 
                && piece.Name == Name 
                && piece.PieceColor == PieceColor)
                return true;
            return false;
        }

        public override int GetHashCode() => (int)Location.File^Location.Rank;

        public abstract List<Location> GetMoves(Board board, Square from);

        public abstract List<Location> GetLocationsAttackedByPiece(Board board, Square attacker);
    }
}
