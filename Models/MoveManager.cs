using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChessWebApp
{
    public class MoveManager
    {
        public MoveValidator MoveValidator;
        public Square AdditionalSquareToUpdate { get; set; }

        public MoveManager()
        {
            MoveValidator = new();
        }

        public bool MakeMove(Board board, Square fromSquare, Square toSquare)
        {
            if (toSquare.CurrentPiece != null) RemoveAttackerFromAllAttackedByPieceOnSquareLists(board, toSquare);
            RemoveAttackerFromAllAttackedByPieceOnSquareLists(board, fromSquare);

            fromSquare.MovePiece(toSquare);

            //castling
            if (toSquare.CurrentPiece is King && Math.Abs(fromSquare.Location.File - toSquare.Location.File) == 2)
                HandleCastling(board, toSquare);

            if (toSquare.CurrentPiece is Pawn pawn)
            {
                //en passant
                if (fromSquare.Location.File != toSquare.Location.File && board.PawnToBeTakenEnPassant is not null)
                    HandleEnPassant(board, toSquare);

                //pawn promotion
                if (pawn.PieceColor == PieceColor.Light && pawn.Location.Rank == 8 
                    || pawn.PieceColor == PieceColor.Dark && pawn.Location.Rank == 1)
                    board.RegisterPawnToPromote(pawn);
            }

            board.ApplyToSquares(sq => UpdateSquaresAttackedByPiece(board, sq));

            board.PerformedMoves.Add(Tuple.Create(fromSquare, toSquare));

            if (board.PawnToBeTakenEnPassant is not null) board.PawnToBeTakenEnPassant = null;

            return true;
        }

        private void HandleEnPassant(Board board, Square toSquare)
        {
            Location candidateLoc = LocationFactory.Build(toSquare.Location, 0,
                                    toSquare.CurrentPiece.PieceColor == PieceColor.Light ? -1 : 1);

            Square candidateSquare = board.LocationSquareMap[candidateLoc];

            if (candidateSquare.CurrentPiece is not null
                && candidateSquare.CurrentPiece == board.PawnToBeTakenEnPassant)
            {
                candidateSquare.Reset();
                AdditionalSquareToUpdate = candidateSquare;
            }
        }

        public void PromotePawn(Board board, AbstractPiece piece)
        {
            var square = board.LocationSquareMap[piece.Location];

            RemoveAttackerFromAllAttackedByPieceOnSquareLists(board, square);

            square.CurrentPiece = piece;

            board.ApplyToSquares(sq => UpdateSquaresAttackedByPiece(board, sq));

            board.SetAllSquaresNotValid();

            board.PawnToPromote = null;
        }

        public void HandleCastling(Board board, Square square)
        {
            Square FromRookSquare = null;
            Square ToRookSquare = null;

            //bottom left
            if (square.Location == new Location(File.C, 1))
            {
                //absent square are validated in king class logic

                if (board.LocationSquareMap[new Location(File.D, 1)].IsOccupied) return;

                FromRookSquare = board.LocationSquareMap[new Location(File.A, 1)];
                ToRookSquare = board.LocationSquareMap[new Location(File.D, 1)];
            }
            //bottom right
            else if (square.Location == new Location(File.G, 1))
            {
                FromRookSquare = board.LocationSquareMap[new Location(File.H, 1)];
                ToRookSquare = board.LocationSquareMap[new Location(File.F, 1)];
            }
            //top left
            else if (square.Location == new Location(File.C, 8))
            {
                FromRookSquare = board.LocationSquareMap[new Location(File.A, 8)];
                ToRookSquare = board.LocationSquareMap[new Location(File.D, 8)];
            }
            //top right
            else if (square.Location == new Location(File.G, 8))
            {
                if (board.LocationSquareMap[new Location(File.F, 8)].IsOccupied) return;

                FromRookSquare = board.LocationSquareMap[new Location(File.H, 8)];
                ToRookSquare = board.LocationSquareMap[new Location(File.F, 8)];
            }

            AdditionalSquareToUpdate = FromRookSquare;
            RemoveAttackerFromAllAttackedByPieceOnSquareLists(board, FromRookSquare);
            FromRookSquare.MovePiece(ToRookSquare);
            UpdateSquaresAttackedByPiece(board, ToRookSquare);
        }


        public void UpdateSquaresAttackedByPiece(Board board, Square attacker)
        {
            if (attacker.CurrentPiece == null) return;

            RemoveAttackerFromAllAttackedByPieceOnSquareLists(board, attacker);

            var attackedLocs = attacker.CurrentPiece.GetLocationsAttackedByPiece(board, attacker);

            attackedLocs.ForEach(loc => board.LocationSquareMap[loc].AttackedByPiecesOnSquares.Add(attacker));
        }


        public void RemoveAttackerFromAllAttackedByPieceOnSquareLists(Board board, Square attacker) =>
            board.LocationSquareMap.Values
                .Where(sq => sq.AttackedByPiecesOnSquares.Contains(attacker)).ToList()
                .ForEach(sq => sq.AttackedByPiecesOnSquares.Remove(attacker));


        public void ClearValidMoves() => MoveValidator.ValidMoves.Clear();


        public void UpdateValidSquares(Board board, King king, Square square)
            => MoveValidator.UpdateValidSquares(board, king, square);


        public List<Location> GetValidMoves() => MoveValidator.ValidMoves;
    }
}
