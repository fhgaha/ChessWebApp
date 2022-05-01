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
        public List<Square> AttackedByPiecesOnSquares { get; set; }
        public bool IsValid { get; set; }
        public bool IsSelected { get; set; }
        public bool IsPreviousLoc { get; set; }

        public Square(Square originalSquare)
        {
            SquareColor = originalSquare.SquareColor;
            Location = LocationFactory.Build(originalSquare.Location, 0, 0);
            CurrentPiece = PieceFactory.BuildPiece(originalSquare.CurrentPiece);
            IsOccupied = originalSquare.IsOccupied;

            AttackedByPiecesOnSquares = new();
            //AttackedByPiecesOnSquares = PieceFactory.GetCopies(originalSquare.AttackedByPiecesOnSquares);
            IsValid = originalSquare.IsValid;
        }

        public Square(SquareColor squareColor, Location location)
        {
            SquareColor = squareColor;
            Location = location;
            IsOccupied = false;
            AttackedByPiecesOnSquares = new List<Square>();
        }

        public void MovePiece(Square to)
        {
            CurrentPiece.IsFirstMove = false;
            to.IsOccupied = true;
            to.CurrentPiece = this.CurrentPiece;
            to.CurrentPiece.Location = to.Location;
            Reset();
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

        public void CopyAttackedByPiecesOnSquares(Board fakeBoard, List<Square> originalSquareAttackersList)
        {
            foreach (Square originalAttacker in originalSquareAttackersList)
            {
                var fakeAttacker = fakeBoard.LocationSquareMap[originalAttacker.Location];
                AttackedByPiecesOnSquares.Add(fakeAttacker);
            }
        }
    }
}
