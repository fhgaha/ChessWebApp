
using ChessWebApp.Models.MoveModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessWebApp
{
    public class MoveManager
    {
        public MoveValidator MoveValidator { get; set; }
        public Square AdditionalSquareToUpdate { get; set; }
        public Stack<Move> UndoStack { get; set; }
        public Stack<Move> RedoStack { get; set; }

        public MoveManager()
        {
            MoveValidator = new();
            UndoStack = new();
            RedoStack = new();
        }

        public bool MakeMove(Board board, Square fromSquare, Square toSquare)
        {
            toSquare = ConvertLichessCastlingToNormal(board, fromSquare, toSquare);

            RemoveAttackerFromAllAttackedByPieceOnSquareLists(board, toSquare);
            RemoveAttackerFromAllAttackedByPieceOnSquareLists(board, fromSquare);

            //add to undo stack
            var move = new Move { From = fromSquare.Location, To = toSquare.Location };
            if (toSquare.CurrentPiece is not null) move.CapturedPiece = toSquare.CurrentPiece;
            UndoStack.Push(move);

            move.WasFirstMove = fromSquare.CurrentPiece.IsFirstMove;
            move.CapturedPiece = toSquare.CurrentPiece;

            if (fromSquare.CurrentPiece is King king)
                move.SaveKingData(king);

            //castling
            if (fromSquare.CurrentPiece is King && Math.Abs(fromSquare.Location.File - toSquare.Location.File) == 2)
                HandleCastling(board, toSquare, move);

            fromSquare.MovePiece(toSquare);

            if (toSquare.CurrentPiece is King _king)
                _king.SetIsAbleToCastle(board);

            //this is for UpdateChangedSquares() view
            SetPreviousMoveSquares(board, move, true);

            //count halfmoves
            if (toSquare.CurrentPiece is Pawn || move?.CapturedPiece is not null)
                board.HalfmoveCount = 0;
            else
                board.HalfmoveCount++;

            HandlePawnMove(board, fromSquare, toSquare, move);

            if (toSquare.CurrentPiece is not Pawn) board.PawnToBeTakenEnPassant = null;
            board.ApplyToSquares(sq => UpdateSquaresAttackedByPiece(board, sq));

            board.LastMove = move;
            board.IsWhitesMove = !board.IsWhitesMove;

            return true;
        }

        private void HandlePawnMove(Board board, Square fromSquare, Square toSquare, Move move)
        {
            var rankDifference = Math.Abs(fromSquare.Location.Rank - toSquare.Location.Rank);

            if (toSquare.CurrentPiece is not Pawn || rankDifference != 2)
                board.PawnToBeTakenEnPassant = null;

            if (toSquare.CurrentPiece is not Pawn pawn) return;

            //set PawnToBeTakenEnPassant 
            board.PawnToBeTakenEnPassant = pawn;
            move.PawnToBeTakenEnPassant = pawn;

            HandleEnPassant(board, fromSquare, toSquare);

            //pawn promotion
            if (pawn.PieceColor == PieceColor.Light && pawn.Location.Rank == 8
                || pawn.PieceColor == PieceColor.Dark && pawn.Location.Rank == 1)
                board.PawnToPromote = pawn;
        }

        public void UndoMove(Board board)
        {
            if (UndoStack.Count == 0) return;

            board.IsWhitesMove = !board.IsWhitesMove;

            var move = UndoStack.Pop();

            Square currentSquare = board.LocationSquareMap[move.To];
            Square originalSquare = board.LocationSquareMap[move.From];

            RemoveAttackerFromAllAttackedByPieceOnSquareLists(board, originalSquare);
            RemoveAttackerFromAllAttackedByPieceOnSquareLists(board, currentSquare);

            currentSquare.MovePiece(originalSquare);

            SetPreviousMoveSquares(board, move, false);

            //if castling state changed bring it back
            if (originalSquare.CurrentPiece is King king)
            {
                move.RestoreKingData(king);

                //should bring king and rook on positions before castling
                UndoCastling(board, move, king);
            }

            //undo propmotion
            if (move.Kind == MoveKind.PawnToQueen 
                || move.Kind == MoveKind.PawnToRook
                || move.Kind == MoveKind.PawnToBishop
                || move.Kind == MoveKind.PawnToKnight)
            {
                originalSquare.CurrentPiece = new Pawn(board.King.PieceColor) 
                { Location = originalSquare.Location, IsFirstMove = false };
            }

            //restore catured piece
            if (move.CapturedPiece is AbstractPiece captured && captured is not null)
            {
                currentSquare.CurrentPiece = captured;
                currentSquare.IsOccupied = true;
            }

            originalSquare.CurrentPiece.IsFirstMove = move.WasFirstMove;

            board.ApplyToSquares(sq => UpdateSquaresAttackedByPiece(board, sq));

            RedoStack.Push(move);
        }

        private void UndoCastling(Board board, Move move, King king)
        {
            if (move.Kind != MoveKind.Castling) return;

            Square rookCurrentSq = board.LocationSquareMap[move.RookPositionAfterCastling];
            Square rookOriginalSq = board.LocationSquareMap[move.RookPositionBeforeCastling];
            AbstractPiece rook = rookCurrentSq.CurrentPiece;

            RemoveAttackerFromAllAttackedByPieceOnSquareLists(board, rookCurrentSq);
            RemoveAttackerFromAllAttackedByPieceOnSquareLists(board, rookOriginalSq);

            rookCurrentSq.MovePiece(rookOriginalSq);
            rook.IsFirstMove = true;
            king.IsFirstMove = true;
        }

        private Square ConvertLichessCastlingToNormal(Board board, Square fromSquare, Square toSquare)
        {
            if (fromSquare.CurrentPiece is King king
                && Math.Abs(fromSquare.Location.File - toSquare.Location.File) >= 2)
            {
                Move move;

                //king's side
                if (king.Location.File - toSquare.Location.File < 0)
                    move = new Move { To = LocationFactory.Build(king.Location, 2, 0) };

                //queen's side
                else
                    move = new Move { To = LocationFactory.Build(king.Location, -2, 0) };

                toSquare = board.LocationSquareMap[move.To];
            }

            return toSquare;
        }

        private void SetPreviousMoveSquares(Board board, Move move, bool value)
        {
            //set all as not previous
            board.LocationSquareMap.Values.ToList()
                .Where(sq => sq.IsPreviousLoc == true).ToList()
                .ForEach(sq => sq.IsPreviousLoc = false);

            var from = board.LocationSquareMap[move.From];
            var to = board.LocationSquareMap[move.To];

            from.IsPreviousLoc = value;
            to.IsPreviousLoc = value;
        }

        private void HandleEnPassant(Board board, Square from, Square to)
        {
            if (from.Location.File == to.Location.File || board.PawnToBeTakenEnPassant is null) return;

            Location candidateLoc = LocationFactory.Build(
                to.Location, 0, to.CurrentPiece.PieceColor == PieceColor.Light ? -1 : 1);

            Square candidateSquare = board.LocationSquareMap[candidateLoc];

            if (candidateSquare.CurrentPiece is not null
                && candidateSquare.CurrentPiece == board.PawnToBeTakenEnPassant)
            {
                candidateSquare.Reset();
                AdditionalSquareToUpdate = candidateSquare;
            }
        }

        //called from controller
        public void PromotePawn(Board board, AbstractPiece piece)
        {
            var square = board.LocationSquareMap[piece.Location];
            RemoveAttackerFromAllAttackedByPieceOnSquareLists(board, square);
            square.CurrentPiece = piece;
            board.ApplyToSquares(sq => UpdateSquaresAttackedByPiece(board, sq));
            board.PawnToPromote = null;

            var move = UndoStack.Peek();

            switch (piece)
            {
                case Queen:
                    move.Kind = MoveKind.PawnToQueen;
                    break;
                case Rook:
                    move.Kind = MoveKind.PawnToRook;
                    break;
                case Bishop:
                    move.Kind = MoveKind.PawnToBishop;
                    break;
                case Knight:
                    move.Kind = MoveKind.PawnToKnight;
                    break;
            }
        }

        public void HandleCastling(Board board, Square to, Move move)
        {
            Square FromRookSquare = null;
            Square ToRookSquare = null;

            //bottom left
            if (to.Location == new Location(File.C, 1))
            {
                //absent square are validated in king class logic

                if (board.LocationSquareMap[new Location(File.D, 1)].IsOccupied) return;

                FromRookSquare = board.LocationSquareMap[new Location(File.A, 1)];
                ToRookSquare = board.LocationSquareMap[new Location(File.D, 1)];
            }
            //bottom right
            else if (to.Location == new Location(File.G, 1))
            {
                FromRookSquare = board.LocationSquareMap[new Location(File.H, 1)];
                ToRookSquare = board.LocationSquareMap[new Location(File.F, 1)];
            }
            //top left
            else if (to.Location == new Location(File.C, 8))
            {
                FromRookSquare = board.LocationSquareMap[new Location(File.A, 8)];
                ToRookSquare = board.LocationSquareMap[new Location(File.D, 8)];
            }
            //top right
            else if (to.Location == new Location(File.G, 8))
            {
                if (board.LocationSquareMap[new Location(File.F, 8)].IsOccupied) return;

                FromRookSquare = board.LocationSquareMap[new Location(File.H, 8)];
                ToRookSquare = board.LocationSquareMap[new Location(File.F, 8)];
            }

            AdditionalSquareToUpdate = FromRookSquare;
            RemoveAttackerFromAllAttackedByPieceOnSquareLists(board, FromRookSquare);
            FromRookSquare.MovePiece(ToRookSquare);
            UpdateSquaresAttackedByPiece(board, ToRookSquare);

            move.Kind = MoveKind.Castling;
            //move.SetNotAbleToCastle(board.King);
            move.RookPositionBeforeCastling = FromRookSquare.Location;
            move.RookPositionAfterCastling = ToRookSquare.Location;
        }

        /// <summary>
        /// Remove attacker from every square's AttackedByPieceOnSquareLists 
        /// then add attacker to squares that are under attack right now
        /// </summary>
        /// <param name="board"></param>
        /// <param name="attacker"></param>
        public void UpdateSquaresAttackedByPiece(Board board, Square attacker)
        {
            if (attacker.CurrentPiece == null) return;

            RemoveAttackerFromAllAttackedByPieceOnSquareLists(board, attacker);

            var attackedLocs = attacker.CurrentPiece.GetLocationsAttackedByPiece(board, attacker);

            attackedLocs.ForEach(loc => board.LocationSquareMap[loc].AttackedByPiecesOnSquares.Add(attacker));
        }

        public void RemoveAttackerFromAllAttackedByPieceOnSquareLists(Board board, Square attacker)
        {
            if (attacker.CurrentPiece is null) return;

            board.LocationSquareMap.Values
                .Where(sq => sq.AttackedByPiecesOnSquares.Contains(attacker)).ToList()
                .ForEach(sq => sq.AttackedByPiecesOnSquares.Remove(attacker));
        }

        public List<Location> GetValidMoves(Board board, King king, Square defender)
            => MoveValidator.GetValidMoves(board, king, defender);

        public List<Move> GenerateMovesForAllPieces(Board board, PieceColor color)
        {
            MoveValidator validator = new();
            List<Move> moves = new();

            foreach (AbstractPiece piece in board.TotalPieces.Where(p => p.PieceColor == color))
            {
                var locations = validator.GetValidMoves(
                    board,
                    color == PieceColor.Light ? board.WhiteKing : board.BlackKing,
                    board.LocationSquareMap[piece.Location]);

                foreach (var loc in locations)
                {
                    Move move = new()
                    {
                        From = piece.Location,
                        To = loc,
                        MovingPiece = piece
                    };

                    moves.Add(move);
                }
            }
            return moves;
        }
    }
}
