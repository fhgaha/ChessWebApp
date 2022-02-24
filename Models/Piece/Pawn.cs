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
        public override List<Location> GetValidMoves(Board board, Square square)
        {
            return GetValidMoves(board);
        }

        public override List<Location> GetValidMoves(Board board)
        {
            var moveCandidates = new List<Location>();
            var squareMap = board.LocationSquareMap;

            if (PieceColor == PieceColor.Light)
            {
                moveCandidates.Add(LocationFactory.Build(CurrentSquare.Location, 0, 1));
                if (isFirstMove) moveCandidates.Add(LocationFactory.Build(CurrentSquare.Location, 0, 2));
                moveCandidates.Add(LocationFactory.Build(CurrentSquare.Location, 1, 1));
                moveCandidates.Add(LocationFactory.Build(CurrentSquare.Location, -1, 1));
            }
            else
            {
                moveCandidates.Add(LocationFactory.Build(CurrentSquare.Location, 0, -1));
                if (isFirstMove) moveCandidates.Add(LocationFactory.Build(CurrentSquare.Location, 0, -2));
                moveCandidates.Add(LocationFactory.Build(CurrentSquare.Location, 1, -1));
                moveCandidates.Add(LocationFactory.Build(CurrentSquare.Location, -1, -1));
            }


            //need en passant logic

            return moveCandidates.Where(candidate =>
            {
                if (!squareMap.ContainsKey(candidate))
                    return false;
                else
                    if (candidate.File == CurrentSquare.Location.File)
                {
                    if (squareMap[candidate].IsOccupied)
                        return false;
                }
                else
                        if (squareMap[candidate].CurrentPiece == null)
                    return false;
                else if (squareMap[candidate].CurrentPiece.PieceColor == PieceColor)
                    return false;

                return true;
            }).ToList();
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
