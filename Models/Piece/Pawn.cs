using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessWebApp
{
    public class Pawn : AbstractPiece
    {
        private bool isFirstMove = true;

        public Pawn(PieceColor pieceColor) : base(pieceColor)
        {
            Name = "Pawn";
        }

        public List<Location> GetValidMoves(Board board)
        {
            var moveCandidates = new List<Location>();
            var squareMap = board.LocationSquareMap;

            moveCandidates.Add(LocationFactory.Build(CurrentSquare.Location, 0, 1));

            if (isFirstMove)
                moveCandidates.Add(LocationFactory.Build(CurrentSquare.Location, 0, 2));

            moveCandidates.Add(LocationFactory.Build(CurrentSquare.Location, 1, 1));
            moveCandidates.Add(LocationFactory.Build(CurrentSquare.Location, -1, 1));

            //need en passant logic

            return moveCandidates.Where(candidate =>
            {
                //filter out locations that do not exist on the board
                if (!squareMap.ContainsKey(candidate)) return false;
                if (candidate.File == CurrentSquare.Location.File && squareMap[candidate].IsOccupied) return false;
                //check if candidate can be captured
                return squareMap[candidate].CurrentPiece.PieceColor != PieceColor;
            }).ToList();
        }

        public List<Location> GetValidMoves(Board board, Square square)
        {
            throw new NotImplementedException();
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
