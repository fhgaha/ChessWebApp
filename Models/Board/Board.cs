using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessWebApp
{
    public class Board
    {
        public const int BoardLength = 8;
        public  Square[,] BoardSquares { get; private set; }
        public Dictionary<Location, Square> LocationSquareMap { get; private set; }
        public  List<AbstractPiece> LightPieces { get; private set; }
        public  List<AbstractPiece> DarkPieces { get; private set; }

        public Board()
        {
            BoardSquares = new Square[BoardLength, BoardLength];
            LightPieces = new List<AbstractPiece>();
            DarkPieces = new List<AbstractPiece>();

            LocationSquareMap = new Dictionary<Location, Square>();
            var pieces = PieceFactory.GetPieces();

            for (int i = 0; i < BoardSquares.GetLength(0); i++)
            {
                var column = 0;
                var currentColor = i % 2 == 0 ? SquareColor.Light : SquareColor.Dark;

                foreach (File file in Enum.GetValues(typeof(File)))
                {
                    var newSquare = new Square(currentColor, new Location(file, BoardLength - i));
                    if (pieces.ContainsKey(newSquare.Location))
                    {
                        var piece = pieces[newSquare.Location];
                        RegisterPieceOnSquare(piece, newSquare);

                        if (piece.PieceColor == PieceColor.Light)
                            LightPieces.Add(piece);
                        else DarkPieces.Add(piece);
                    }
                    BoardSquares[i, column] = newSquare;
                    LocationSquareMap.Add(newSquare.Location, newSquare);
                    currentColor = currentColor == SquareColor.Light ? SquareColor.Dark : SquareColor.Light;
                    column++;

                }
            }
        }

        public void PrintBoard()
        {
            for (int i = 0; i < BoardSquares.GetLength(0); i++)
            {
                Console.Write(BoardLength - i + " ");
                for (int j = 0; j < BoardSquares.GetLength(1); j++)
                {
                    if (BoardSquares[i, j].IsOccupied)
                    {
                        var piece = BoardSquares[i, j].CurrentPiece;
                        if (piece is Knight) Console.Write("N ");
                        else Console.Write(piece.Name[0] + " ");
                    }
                    else Console.Write("- ");
                }
                Console.WriteLine();
            }

            Console.Write("  ");
            foreach (var file in Enum.GetValues(typeof(File)))
                Console.Write(file + " ");
            Console.WriteLine("\n");
        }

        private void RegisterPieceOnSquare(AbstractPiece piece, Square newSquare)
        {
            var previousSquare = piece.CurrentSquare;
            if (previousSquare != null)
            {
                previousSquare.IsOccupied = false;
                previousSquare.CurrentPiece = null;
            }

            newSquare.CurrentPiece = piece;
            newSquare.IsOccupied = true;

            piece.CurrentSquare = newSquare;
        }
    }
}
