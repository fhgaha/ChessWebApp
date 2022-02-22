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

        public List<Location> GetValidMoves(Board board)
        {
            throw new NotImplementedException();
        }

        public List<Location> GetValidMoves(Board board, Square square)
        {
            throw new NotImplementedException();
        }

        public virtual void MakeMove(Square square)
        {
            
        }
    }
}
