using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessWebApp
{
    public class LocationFactory
    {
        private static readonly File[] files = (File[])Enum.GetValues(typeof(File));

        public static Location Build(Location current, int fileOffset, int rankOffset)
        {
            int currentFile = (int)current.File;
            int newFileValue = Math.Clamp(currentFile + fileOffset, 0, Constants.BoardLength - 1);
            int newRankValue = Math.Clamp(current.Rank + rankOffset, 0, Constants.BoardLength - 1);
            return new Location(files[newFileValue], newRankValue);
        }
    }
}
