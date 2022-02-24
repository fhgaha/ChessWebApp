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
        public bool IsValid { get; set; }

        public Square(SquareColor squareColor, Location location)
        {
            SquareColor = squareColor;
            Location = location;
            IsOccupied = false;
            IsValid = false;
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
