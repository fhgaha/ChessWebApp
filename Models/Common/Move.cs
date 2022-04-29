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
    }
}
