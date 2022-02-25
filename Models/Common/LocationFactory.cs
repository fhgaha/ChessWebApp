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

            int newFileValue = currentFile + fileOffset;
            if (newFileValue < 0 || newFileValue > Constants.BoardLength - 1) 
                return null;

            int newRankValue = current.Rank + rankOffset;
            if (newRankValue < 1 || newRankValue > Constants.BoardLength) 
                return null;

            return new Location(files[newFileValue], newRankValue);
        }
    }
}
