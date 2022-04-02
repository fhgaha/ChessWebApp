using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessWebApp
{
    public class Board
    {
        public bool IsReal = true;
        public List<Tuple<Square, Square>> PerformedMoves = new List<Tuple<Square, Square>>();
        public Square[,] BoardSquares { get; private set; }
        public Dictionary<Location, Square> LocationSquareMap { get; private set; }
        public List<AbstractPiece> LightPieces { get; private set; }
        public List<AbstractPiece> DarkPieces { get; private set; }
        public King WhiteKing { get; private set; }
        public King BlackKing { get; private set; }
        public Pawn PawnToPromote;
        public string message = "";

        public Board()
        {
            BoardSquares = new Square[Constants.BoardLength, Constants.BoardLength];
            LightPieces = new List<AbstractPiece>();
            DarkPieces = new List<AbstractPiece>();
            LocationSquareMap = new Dictionary<Location, Square>();

            var pieces =
                //PieceFactory.GetStandartPiecePositions();
                //PieceFactory.GetTwoKings();
                //PieceFactory.GetCastlingSetup();
                PieceFactory.GetPromotionSetup();

            for (int i = 0; i < BoardSquares.GetLength(0); i++)
            {
                var column = 0;
                var currentColor = i % 2 == 0 ? SquareColor.Light : SquareColor.Dark;

                foreach (File file in Enum.GetValues(typeof(File)))
                {
                    var newSquare = new Square(currentColor, new Location(file, Constants.BoardLength - i));
                    if (pieces.ContainsKey(newSquare.Location))
                    {
                        var piece = pieces[newSquare.Location];
                        piece.Location = newSquare.Location;
                        newSquare.CurrentPiece = piece;
                        newSquare.IsOccupied = true;

                        if (piece.PieceColor == PieceColor.Light)
                        {
                            LightPieces.Add(piece);
                            if (piece is King) WhiteKing = (King)piece;
                        }
                        else
                        {
                            DarkPieces.Add(piece);
                            if (piece is King) BlackKing = (King)piece;
                        }
                    }
                    BoardSquares[i, column] = newSquare;
                    LocationSquareMap.Add(newSquare.Location, newSquare);
                    currentColor = currentColor == SquareColor.Light ? SquareColor.Dark : SquareColor.Light;
                    column++;
                }
            }
        }


        public void SetAllSquaresNotValid() => LocationSquareMap.Values.ToList().ForEach(sq => sq.IsValid = false);

        public void PrintBoard()
        {
            for (int i = 0; i < BoardSquares.GetLength(0); i++)
            {
                Console.Write(Constants.BoardLength - i + " ");
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

        internal void RegisterPawnToPromote(Pawn pawn) => PawnToPromote = pawn;
        internal void ApplyToAll(Action<Square> action) => LocationSquareMap.Values.ToList().ForEach(action);

    }
}
