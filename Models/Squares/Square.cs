using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessWebApp
{
    public class Square
    {
        public SquareColor SquareColor { get; }
        public Location Location { get; }
        public AbstractPiece CurrentPiece { get; set; }
        public bool IsOccupied { get; set; }
        public List<AbstractPiece> AttackedByPieces { get; set; }
        public bool IsValid { get; set; }

        public Square(Square square)
        {
            SquareColor = square.SquareColor;
            Location = LocationFactory.Build(square.Location, 0, 0);
            CurrentPiece = square.CurrentPiece;
            IsOccupied = square.IsOccupied;

            AttackedByPieces = new List<AbstractPiece>();
            square.AttackedByPieces.ForEach(p =>AttackedByPieces.Add(p));
            IsValid = square.IsValid;
        }

        public Square(SquareColor squareColor, Location location)
        {
            SquareColor = squareColor;
            Location = location;
            IsOccupied = false;
            AttackedByPieces = new List<AbstractPiece>();
        }

        public void Reset()
        {
            IsOccupied = false;
            CurrentPiece = null;
        }

        public override string ToString()
        {
            return "Square{" +
                "squareColor = " + SquareColor +
                ", location = " + Location +
                ", isOccupied = " + IsOccupied +
                ", isValid = " + IsValid +
                "}";
        }
    }
}
