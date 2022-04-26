using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessWebApp
{
    public interface IMovable
    {
        List<Location> GetMoves(Board board, Square square);
    }
}
