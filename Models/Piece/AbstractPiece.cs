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
        public Square CurrentSquare { get; set; }
        public bool isFirstMove = true;


        protected AbstractPiece(PieceColor pieceColor)
        {
            PieceColor = pieceColor;
        }

        public override string ToString()
        {
            return "AbstractPiece{" +
                "name = " + Name +
                ", pieceColor = " + PieceColor +
                ", currentSquare = " + CurrentSquare +
                "}";
        }

        public abstract List<Location> GetValidMoves(Board board);

        public abstract List<Location> GetValidMoves(Board board, Square square);

        public virtual void MovePiece(Square square) { }

        public abstract List<Location> GetLocationsAttackedByPiece(Board board);
    }
}
