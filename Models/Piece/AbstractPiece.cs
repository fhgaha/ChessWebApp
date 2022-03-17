using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessWebApp
{
    public abstract class AbstractPiece : IMovable
    {
        private static int idCount = 0;
        public int Id { get; }
        public string Name { get; protected set; }
        public PieceColor PieceColor { get; protected set; }
        public bool isFirstMove = true;
        public Location Location { get; set; }

        protected AbstractPiece(PieceColor pieceColor)
        {
            PieceColor = pieceColor;
            idCount++;
            Id = idCount;
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

            if (((AbstractPiece)obj).Id == Id)
                return true;
            return false;
        }

        public override int GetHashCode() => Id;

        public abstract List<Location> GetValidMoves(Board board, Square from);

        public abstract List<Location> GetLocationsAttackedByPiece(Board board, Square attacker);
    }
}
