﻿using ChessWebApp.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        }

        public bool MakeMove(Board board, Square fromSquare, Square toSquare)
        {
            toSquare = ConvertLichessCastlingToNormal(board, fromSquare, toSquare);

            RemoveAttackerFromAllAttackedByPieceOnSquareLists(board, toSquare);
            RemoveAttackerFromAllAttackedByPieceOnSquareLists(board, fromSquare);

            //add to undo stack
            var move = new Move { From = fromSquare.Location, To = toSquare.Location };
            if (toSquare.CurrentPiece is not null)
                move.CapturedPiece = toSquare.CurrentPiece;

            UndoStack.Push(move);

            fromSquare.MovePiece(toSquare);
            DoAfterMove(board, fromSquare, toSquare, move);

            return true;
        }

        private void DoAfterMove(Board board, Square fromSquare, Square toSquare, Move move)
        {
            //this is for UpdateChangedSquares() view
            SetPreviousMoveSquares(board);

            if (toSquare.CurrentPiece is King king)
            {
                king.isAbleToCastleKingSide = king.isAbleToCastleQueenSide = false;
                move.SetNotAbleToCastle(king);
            }

            //castling
            if (toSquare.CurrentPiece is King && Math.Abs(fromSquare.Location.File - toSquare.Location.File) == 2)
            {
                HandleCastling(board, toSquare, move);
            }

            //count halfmoves
            if (toSquare.CurrentPiece is Pawn || move?.CapturedPiece is not null)
                board.HalfmoveCount = 0;
            else
                board.HalfmoveCount++;

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

            //!
            board.PerformedMoves.Add(new Move { From = fromSquare.Location, To = toSquare.Location });

            if (board.PawnToBeTakenEnPassant is not null) board.PawnToBeTakenEnPassant = null;

            board.IsWhitesMove = !board.IsWhitesMove;
        }

        public void UndoMove(Board board)
        {
            var move = UndoStack.Pop();
            Square currentSquare = board.LocationSquareMap[move.To];
            Square originalSquare = board.LocationSquareMap[move.From];

            RemoveAttackerFromAllAttackedByPieceOnSquareLists(board, originalSquare);
            RemoveAttackerFromAllAttackedByPieceOnSquareLists(board, currentSquare);

            currentSquare.MovePiece(originalSquare);

            SetPreviousMoveSquares(board);





            RedoStack.Push(move);
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

        private void SetPreviousMoveSquares(Board board)
        {
            //set all as not previous
            board.LocationSquareMap.Values.ToList()
                .Where(sq => sq.IsPreviousLoc == true).ToList()
                .ForEach(sq => sq.IsPreviousLoc = false);

            var move = UndoStack.Peek();
            
            var from = board.LocationSquareMap[move.From];
            var to = board.LocationSquareMap[move.To];

            from.IsPreviousLoc = true;
            to.IsPreviousLoc = true;
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

            //board.SetAllSquaresNotValid();

            board.PawnToPromote = null;
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

            move.SetNotAbleToCastle(board.King);
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
                    Move move = new Move
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
