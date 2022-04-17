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
            if (newFileValue < 0 || newFileValue > Global.BoardLength - 1) 
                return null;

            int newRankValue = current.Rank + rankOffset;
            if (newRankValue < 1 || newRankValue > Global.BoardLength) 
                return null;

            return new Location(files[newFileValue], newRankValue);
        }

        public static Location Parse(string location)
        {
            if (location is null) return null;

            File file = Array.Find(
                (File[])Enum.GetValues(typeof(File)),
                f => f.ToString() == location[0].ToString());

            int rank = int.Parse(location[1].ToString());

            return new Location(file, rank);
        }
    }
}
