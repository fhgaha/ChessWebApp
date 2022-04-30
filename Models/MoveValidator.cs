using System.Collections.Generic;
using System.Linq;

namespace ChessWebApp
{
    public class MoveValidator
    {
        //public List<Location> ValidMovesToDisplay { get; private set; } = new();

        //public void UpdateValidSquares(Board board, King king, Square square)
        //{
        //    var legalMoves = GetValidMoves(board, king, square);

        //    //board.SetAllSquaresNotValid();
        //    //legalMoves.ForEach(loc => board.LocationSquareMap[loc].IsValid = true);
        //    ValidMovesToDisplay = legalMoves;
        //}

        public List<Location> GetValidMoves(Board board, King king, Square defender)
        {
            var moves = defender.CurrentPiece.GetMoves(board, defender);
            moves = FilterMovesToPreventCheck(board, moves, defender.CurrentPiece, king);

            return moves;
        }

        public List<Location> FilterMovesToPreventCheck(Board originalBoard, List<Location> candidates,
            AbstractPiece defender, King king)
        {
            List<Location> defendingMoves = new List<Location>();

            foreach (Location candidate in candidates)
            {
                Board fBoard = new Board() { IsReal = false };

                originalBoard.LocationSquareMap.Keys.ToList()
                    .ForEach(loc => fBoard.LocationSquareMap[loc] = new Square(originalBoard.LocationSquareMap[loc]));

                //copy attacked by pieces on squares list for each square
                foreach (Location loc in fBoard.LocationSquareMap.Keys)
                {
                    var originalSquareAttackersList = originalBoard.LocationSquareMap[loc].AttackedByPiecesOnSquares;
                    var fakeSquareAttackersList = fBoard.LocationSquareMap[loc].AttackedByPiecesOnSquares;

                    fBoard.LocationSquareMap[loc].CopyAttackedByPiecesOnSquares(fBoard, originalSquareAttackersList);
                }

                Square squareCandidate = fBoard.LocationSquareMap[candidate];
                Square fDefenderSquare = fBoard.LocationSquareMap[defender.Location];
                Square fKingSquare = fBoard.LocationSquareMap[king.Location];
                var originalkingAttackersList = originalBoard.LocationSquareMap[king.Location].AttackedByPiecesOnSquares;
                fKingSquare.CopyAttackedByPiecesOnSquares(fBoard, originalkingAttackersList);

                MakeFakeMove(fDefenderSquare, squareCandidate, defendingMoves, candidate, fBoard,
                    (King)fKingSquare.CurrentPiece);
            }
            return defendingMoves;
        }

        public void MakeFakeMove(Square from, Square to, List<Location> defendingMoves,
            Location candidate, Board fBoard, King fKing)
        {
            //if candidate contains attacker, after moving a defender the attaker would be taken.
            //update king's square for every other attacker and check if king is still attacked

            MoveManager fMoveManager = new();
            Square fKingSquare = fBoard.LocationSquareMap[fKing.Location];

            fMoveManager.RemoveAttackerFromAllAttackedByPieceOnSquareLists(fBoard, from);
            from.MovePiece(to);
            fBoard.LocationSquareMap.Values.ToList()
                .ForEach(sq => fMoveManager.UpdateSquaresAttackedByPiece(fBoard, sq));

            if (to.IsOccupied && fKingSquare.AttackedByPiecesOnSquares.Contains(to))
            {
                fKingSquare.AttackedByPiecesOnSquares
                    .Where(p => p.Location != candidate).ToList()
                    .ForEach(attacker => fMoveManager.UpdateSquaresAttackedByPiece(fBoard, attacker));
            }
            else
            {
                fBoard.LocationSquareMap.Values
                    .Where(sq => sq.CurrentPiece != null).ToList()
                    .ForEach(sq => fMoveManager.UpdateSquaresAttackedByPiece(fBoard, sq));

                List<Square> attackers = new();
                attackers.AddRange(fKingSquare.AttackedByPiecesOnSquares);
                attackers.ForEach(attacker => fMoveManager.UpdateSquaresAttackedByPiece(fBoard, attacker));
            }

            fKingSquare = fBoard.LocationSquareMap[fKing.Location];
            if (fKing.UpdateIsUnderCheck(fKingSquare) == false)
                defendingMoves.Add(candidate);
        }
    }
}