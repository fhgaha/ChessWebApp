using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChessWebApp.Models.Common
{
    public enum MoveKind
    {
        //will not tie to capturing here, look at CapturedPiece field
        Simple,
        PawnToQueen,
        PawnToRook,
        PawnToKnight,
        PawnToBishop
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
        public string[] WhiteCastlingAbility { get; set; } = new string[2];
        public string[] BlackCastlingAbility { get; set; } = new string[2];

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
                for (int i = 0; i < WhiteCastlingAbility.Length; i++)
                    WhiteCastlingAbility[i] = "-";
            else
                for (int i = 0; i < BlackCastlingAbility.Length; i++)
                    BlackCastlingAbility[i] = "-";
        }
    }
}
