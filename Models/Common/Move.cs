using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChessWebApp.Models.Common
{
    public class Move
    {
        public Location From { get; set; }
        public Location To { get; set; }
        public AbstractPiece MovingPiece { get; set; }
        public AbstractPiece CapturedPiece { get; set; }
        public bool PerformedByWhites { get; set; }
        public int Score { get; set; }

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
    }
}
