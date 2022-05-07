using System.Collections.Generic;

namespace ChessWebApp.Models.MoveModel
{
    public enum MoveKind
    {
        //will not tie to capturing here, look at CapturedPiece field
        Simple,
        PawnToQueen,
        PawnToRook,
        PawnToKnight,
        PawnToBishop,
        //castling ability can be changed by kings or both rooks move so in case of actual castling 
        //look at this value
        Castling
    }

    public enum CastlingAbilityEnum
    {
        WhiteKingSide,
        WhiteQueenSide,
        BlackKingSide,
        BlackQueenSide
    }

    public class Move
    {
        public MoveKind Kind { get; set; }
        public Location From { get; set; }
        public Location To { get; set; }
        public AbstractPiece MovingPiece { get; set; }
        public AbstractPiece CapturedPiece { get; set; }
        public bool PerformedByWhites { get; set; }
        public int Score { get; set; }
        public Dictionary<CastlingAbilityEnum, bool> CastlingAbilityBefore { get; set; } 
            = new Dictionary<CastlingAbilityEnum, bool> 
        {
            [CastlingAbilityEnum.WhiteKingSide] = true,
            [CastlingAbilityEnum.WhiteQueenSide] = true,
            [CastlingAbilityEnum.BlackKingSide] = true,
            [CastlingAbilityEnum.BlackQueenSide] = true,
        };
        public Location RookPositionBeforeCastling { get; set; }
        public Location RookPositionAfterCastling { get; set; }
        public Pawn PawnToBeTakenEnPassant { get; set; }

        public override string ToString()
        {
            return "Move{" +
                "Score = " + Score +
                ", From = " + From +
                ", To = " + To +
                "}";
        }

        //d2d4
        internal static Move Parse(string val)
        {
            return new Move
            {
                From = LocationFactory.Parse(string.Concat(val[0], val[1])),
                To = LocationFactory.Parse(string.Concat(val[2], val[3]))
            };
        }

        public void SetNotAbleToCastle(King king)
        {
            if (king.PieceColor == PieceColor.Light)
            {
                CastlingAbilityBefore[CastlingAbilityEnum.WhiteKingSide] = false;
                CastlingAbilityBefore[CastlingAbilityEnum.WhiteQueenSide] = false;
                return;
            }

            CastlingAbilityBefore[CastlingAbilityEnum.BlackKingSide] = false;
            CastlingAbilityBefore[CastlingAbilityEnum.BlackQueenSide] = false;
        }

        public void SetKingData(King king)
        {
            var castlingAbilityKingSide = king.PieceColor == PieceColor.Light
                    ? CastlingAbilityEnum.WhiteKingSide : CastlingAbilityEnum.BlackKingSide;
            var castlingAbilityQueenSide = king.PieceColor == PieceColor.Light
                ? CastlingAbilityEnum.WhiteQueenSide : CastlingAbilityEnum.BlackQueenSide;

            king.IsAbleToCastleKingSide = CastlingAbilityBefore[castlingAbilityKingSide];
            king.IsAbleToCastleQueenSide = CastlingAbilityBefore[castlingAbilityQueenSide];
        }

        public void SaveKingData(King king)
        {
            var castlingAbilityKingSide = king.PieceColor == PieceColor.Light
                    ? CastlingAbilityEnum.WhiteKingSide : CastlingAbilityEnum.BlackKingSide;
            var castlingAbilityQueenSide = king.PieceColor == PieceColor.Light
                ? CastlingAbilityEnum.WhiteQueenSide : CastlingAbilityEnum.BlackQueenSide;

            CastlingAbilityBefore[castlingAbilityKingSide] = king.IsAbleToCastleKingSide;
            CastlingAbilityBefore[castlingAbilityQueenSide] = king.IsAbleToCastleQueenSide;
        }
    }
}
