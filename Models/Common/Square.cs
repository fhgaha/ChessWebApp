using System;

namespace ChessWebApp
{
    public class Location
    {
        /// <summary>
        ///file is for coloumn in chess
        /// </summary>
        public File File { get; }
        /// <summary>
        ///rank is for row in chess
        /// </summary>
        public int Rank { get; }

        public Location(File file, int rank)
        {
            File = file;
            Rank = rank;
        }

        public override bool Equals(Object obj)
        {
            if (obj == null) return false;
            if (Object.ReferenceEquals(this, obj)) return true;
            if (GetType() != obj.GetType()) return false;

            var location = (Location)obj;
            return location.File == File && location.Rank == Rank;
        }

        public static bool operator ==(Location first, Location second)
        {
            if (first is null)
            {
                if (second is null) return true;
                return false;
            }

            return first.Equals(second);
        }

        public static bool operator !=(Location first, Location second) => !(first == second);

        public override int GetHashCode() => Tuple.Create(File, Rank).GetHashCode();

        public override string ToString()
        {
            return "" + File + Rank + "";
        }
    }
}
