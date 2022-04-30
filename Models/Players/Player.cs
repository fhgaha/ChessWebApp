using ChessWebApp.Models.Engine;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChessWebApp.Models.Players
{
    public interface Player
    {
        public int Score { get; set; }
        public List<AbstractPiece> CapturedPieces { get; set; }

        bool TryMakeMove(Game game, MoveManager moveManager, Square square);
    }
}
