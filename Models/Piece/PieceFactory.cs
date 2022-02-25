using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessWebApp
{
    public sealed class PieceFactory
    {

        public static Dictionary<Location, AbstractPiece> GetStandartPiecePositions()
        {
            var pieces = new Dictionary<Location, AbstractPiece>();

            //rooks
            pieces.Add(new Location(File.A, 1), new Rook(PieceColor.Light));
            pieces.Add(new Location(File.H, 1), new Rook(PieceColor.Light));
            pieces.Add(new Location(File.A, 8), new Rook(PieceColor.Dark));
            pieces.Add(new Location(File.H, 8), new Rook(PieceColor.Dark));

            //knights
            pieces.Add(new Location(File.B, 1), new Knight(PieceColor.Light));
            pieces.Add(new Location(File.G, 1), new Knight(PieceColor.Light));
            pieces.Add(new Location(File.B, 8), new Knight(PieceColor.Dark));
            pieces.Add(new Location(File.G, 8), new Knight(PieceColor.Dark));

            //bishops
            pieces.Add(new Location(File.C, 1), new Bishop(PieceColor.Light));
            pieces.Add(new Location(File.F, 1), new Bishop(PieceColor.Light));
            pieces.Add(new Location(File.C, 8), new Bishop(PieceColor.Dark));
            pieces.Add(new Location(File.F, 8), new Bishop(PieceColor.Dark));

            //queens
            pieces.Add(new Location(File.D, 1), new Queen(PieceColor.Light));
            pieces.Add(new Location(File.D, 8), new Queen(PieceColor.Dark));

            //kings
            pieces.Add(new Location(File.E, 1), new King(PieceColor.Light));
            pieces.Add(new Location(File.E, 8), new King(PieceColor.Dark));

            //pawns
            foreach (File file in Enum.GetValues(typeof(File)))
            {
                pieces.Add(new Location(file, 2), new Pawn(PieceColor.Light));
                pieces.Add(new Location(file, 7), new Pawn(PieceColor.Dark));
            }

            return pieces;
        }

    }
}
