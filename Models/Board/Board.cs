using ChessWebApp.Models.MoveModel;
using ChessWebApp.Models.Notation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessWebApp
{
    public class Board
    {
        public bool IsReal { get; set; } = true;
        public Move LastMove { get; set; }
        public Square[,] Squares { get; private set; }
        public Dictionary<Location, Square> LocationSquareMap { get; private set; }
        public King WhiteKing { get; private set; }
        public King BlackKing { get; private set; }
        public King King => IsWhitesMove ? WhiteKing : BlackKing;
        public Pawn PawnToPromote { get; set; }
        public Pawn PawnToBeTakenEnPassant { get; set; }
        public bool IsWhitesMove { get; set; } = true;
        public int HalfmoveCount { get; set; } = 0;
        public int FullMoveCount { get => HalfmoveCount/2; }
        public string message = "";
        public IEnumerable<AbstractPiece> TotalPieces 
            => LocationSquareMap.Values.Select(sq => sq.CurrentPiece).Where(p => p is not null);
        public IEnumerable<AbstractPiece> LightPieces => TotalPieces.Where(p => p.PieceColor == PieceColor.Light);
        public IEnumerable<AbstractPiece> DarkPieces => TotalPieces.Where(p => p.PieceColor == PieceColor.Dark);

        public Board() : this(PieceFactory.GetStandartPiecePositions()) { }

        public Board(Dictionary<Location, AbstractPiece> pieces)
        {
            Squares = new Square[Global.BoardLength, Global.BoardLength];
            LocationSquareMap = new Dictionary<Location, Square>();

            for (int i = 0; i < Squares.GetLength(0); i++)
            {
                var column = 0;
                var currentColor = i % 2 == 0 ? SquareColor.Light : SquareColor.Dark;

                foreach (File file in Enum.GetValues(typeof(File)))
                {
                    var newSquare = new Square(currentColor, new Location(file, Global.BoardLength - i));
                    if (pieces.ContainsKey(newSquare.Location))
                    {
                        var piece = pieces[newSquare.Location];
                        piece.Location = newSquare.Location;
                        newSquare.CurrentPiece = piece;
                        newSquare.IsOccupied = true;

                        if (piece.PieceColor == PieceColor.Light && piece is King)
                            WhiteKing = (King)piece;

                        if (piece.PieceColor == PieceColor.Dark && piece is King)
                            BlackKing = (King)piece;
                    }
                    Squares[i, column] = newSquare;
                    LocationSquareMap.Add(newSquare.Location, newSquare);
                    currentColor = currentColor == SquareColor.Light ? SquareColor.Dark : SquareColor.Light;
                    column++;
                }
            }
        }


        public void PrintBoard()
        {
            for (int i = 0; i < Squares.GetLength(0); i++)
            {
                Console.Write(Global.BoardLength - i + " ");
                for (int j = 0; j < Squares.GetLength(1); j++)
                {
                    if (Squares[i, j].IsOccupied)
                    {
                        var piece = Squares[i, j].CurrentPiece;
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

        public void RegisterPawnToPromote(Pawn pawn) => PawnToPromote = pawn;

        public void ApplyToSquares(Action<Square> action) => LocationSquareMap.Values.ToList().ForEach(action);

        public Board Copy() => new Fen().Parse(new Fen().Parse(this));
    }
}
