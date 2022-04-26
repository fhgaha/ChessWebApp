using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessWebApp.Models.Notation
{
    /*
        rnbqkbnr — расположение фигур на 8-й горизонтали слева направо,
        / — разделитель,
        pppppppp — расположение фигур на 7-й горизонтали,
        8/8/8/8 — пустые 6-5-4-3-я горизонтали,
        PPPPPPPP — расположение фигур на 2-й горизонтали,
        RNBQKBNR — расположение фигур на 1-й горизонтали,
        w — предстоит ход белых,
        KQkq — возможны короткие и длинные рокировки белых и чёрных,
        - — не было предыдущего хода пешкой на два поля,
        0 — 50-move draw rule. When this counter reaches 100 (allowing each player to make 50 moves), 
            the game ends in a draw
        1 — предстоит первый ход.

        Позиция после хода 1. e4: rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1
        После хода 1. ... d5: rnbqkbnr/ppp1pppp/8/3p4/4P3/8/PPPP1PPP/RNBQKBNR w KQkq d6 0 2
        После хода 2. Nf3: rnbqkbnr/ppp1pppp/8/3p4/4P3/5N2/PPPP1PPP/RNBQKB1R b KQkq - 1 2
        После хода 2. ... Kd7: rnbq1bnr/pppkpppp/8/3p4/4P3/5N2/PPPP1PPP/RNBQKB1R w KQ - 2 3
        */
    public class Fen
    {
        public string Default { get; set; } = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        public static Dictionary<char, Type> NotationMap = new Dictionary<char, Type>
        {
            ['q'] = typeof(Queen),
            ['k'] = typeof(King),
            ['n'] = typeof(Knight),
            ['r'] = typeof(Rook),
            ['b'] = typeof(Bishop),
            ['p'] = typeof(Pawn)
        };

        public string Parse(Board board)
        {
            StringBuilder builder = new();
            int emptySquareCount = 0;

            for (int row = 0; row < Global.BoardLength; row++)
            {
                for (int col = 0; col < Global.BoardLength; col++)
                {
                    var square = board.BoardSquares[row, col];

                    if (!square.IsOccupied)
                    {
                        emptySquareCount++;
                        continue;
                    }

                    emptySquareCount = TryWriteNumber(builder, emptySquareCount);

                    var letter = GetLetter(square.CurrentPiece);
                    builder.Append(letter);
                }

                emptySquareCount = TryWriteNumber(builder, emptySquareCount);
                builder.Append('/');
            }

            builder.Remove(builder.Length - 1, 1);

            builder.Append(string.Join(' ', new[]
            {
                GetWhosMoveIsNext(board),
                GetCastlingLetters(board),
                GetEnPassantCandidate(board),
                "-",
                Get50MoveDrawCount(board),
                GetFullmovesCount(board)
            }.Where(e => e != null)
            ));

            return builder.ToString();
        }


        private string GetLetter(AbstractPiece piece)
        {
            //probably class will always be AbstractPiece
            string letter = piece.GetType().Name.First().ToString();

            if (piece.GetType() == typeof(Knight)) letter = "n";

            letter = piece.PieceColor == PieceColor.Light ? letter.ToUpper() : letter.ToLower();

            return letter;
        }

        private static int TryWriteNumber(StringBuilder builder, int emptySquareCount)
        {
            if (emptySquareCount != 0)
            {
                builder.Append(emptySquareCount);
                emptySquareCount = 0;
            }

            return emptySquareCount;
        }

        private string GetWhosMoveIsNext(Board board)
        {
            //why dont i bind this to Board.IsWhitesMove?
            bool lastMovePieceColorIsWhite = board.LastMove?.Item2.CurrentPiece.PieceColor == PieceColor.Light;
            return lastMovePieceColorIsWhite ? "b" : " w";
        }

        private string GetCastlingLetters(Board board)
        {
            string result = "";

            //short castling for white king
            if (board.WhiteKing.isAbleToCastleKingSide
                && board.LocationSquareMap[new Location(File.H, 1)].CurrentPiece is Rook rookH
                && rookH is not null
                && rookH.IsFirstMove)
                result += "K";

            //long castling for white king
            if (board.WhiteKing.isAbleToCastleQueenSide
                && board.LocationSquareMap[new Location(File.A, 1)].CurrentPiece is Rook rookA
                && rookA is not null
                && rookA.IsFirstMove)
                result += "Q";

            //short castling for black king
            if (board.BlackKing.isAbleToCastleKingSide
                && board.LocationSquareMap[new Location(File.H, 8)].CurrentPiece is Rook rookH_
                && rookH_ is not null
                && rookH_.IsFirstMove)
                result += "k";

            //long castling for black king
            if (board.BlackKing.isAbleToCastleQueenSide
                && board.LocationSquareMap[new Location(File.A, 8)].CurrentPiece is Rook rookA_
                && rookA_ is not null
                && rookA_.IsFirstMove)
                result += "q";

            return result == "" ? "-" : result;
        }

        private string GetEnPassantCandidate(Board board)
        {
            if (board.PawnToBeTakenEnPassant is null) return "";
            return LocationFactory.Parse(board.PawnToBeTakenEnPassant.Location);
        }

        private string Get50MoveDrawCount(Board board) => board.HalfmoveCount.ToString();

        private string GetFullmovesCount(Board board) => (board.PerformedMoves.Count / 2).ToString();


        internal Board Parse(string fenValue)
        {
            //rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1

            var pieces = new Dictionary<Location, AbstractPiece>();

            var rows = fenValue.Split('/', ' ').Take(8).ToList();
            var whosMoveIsNext = fenValue.Split('/', ' ').Skip(8).First();
            var castlingAbility = fenValue.Split('/', ' ').Skip(9).First();
            var halfMovesCount = fenValue.Split('/', ' ').SkipLast(1).Last();
            var fullMovesCount = fenValue.Split('/', ' ').Last();


            for (int i = 0; i < 8; i++)
            {
                if (rows[i] == "8") continue;

                int rank = 8 - i;
                int file = 0;

                for (int j = 0; j < rows[i].Length; j++)
                {
                    char c = rows[i][j];

                    if (!char.IsDigit(c))
                    {
                        var currentColor = char.IsUpper(c) ? PieceColor.Light : PieceColor.Dark;
                        var currentType = NotationMap[char.ToLowerInvariant(c)];
                        var currentLocation = new Location((File)file, rank);

                        pieces.Add(
                            currentLocation,
                            (AbstractPiece)Activator.CreateInstance(currentType, currentColor));
                    }
                    else
                        file += int.Parse(c.ToString()) - 1;

                    file++;
                }
            }

            Board board = new(pieces);

            //w
            board.IsWhitesMove = whosMoveIsNext == "w" ? true : false;

            //KQkq
            if (!castlingAbility.Contains('K')) board.WhiteKing.isAbleToCastleKingSide = false;
            if (!castlingAbility.Contains('Q')) board.WhiteKing.isAbleToCastleQueenSide = false;
            if (!castlingAbility.Contains('k')) board.BlackKing.isAbleToCastleKingSide = false;
            if (!castlingAbility.Contains('q')) board.BlackKing.isAbleToCastleKingSide = false;

            //halfmoves
            board.HalfmoveCount = int.Parse(halfMovesCount);

            //fullmoves


            //return PieceFactory.GetStandartPiecePositions();
            return board;
        }

        internal bool ValidateInput(string input)
        {
            //rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1

            var rows = input.Split('/', ' ').Take(8);
            var whosMoveIsNext = input.Split('/', ' ').Skip(8).First();
            var castlingAbility = input.Split('/', ' ').Skip(9).First();
            var halfMovesCount = input.Split('/', ' ').SkipLast(1).Last();
            var fullMovesCount = input.Split('/', ' ').Last();

            //input should have 7 '/' symbols
            if (input.Trim('/', ' ').Count(c => c == '/') != 7) return false;

            //input should contain 5 spaces
            if (input.Trim('/', ' ').Count(c => c == ' ') != 5) return false;

            foreach (string row in rows)
            {
                //row with one character should be digit and should have value '8' 
                if (row.Length == 1 && (!int.TryParse(row, out _) || int.Parse(row) != 8)) return false;

                //row should only contain digits or piece first letter
                var pieceAbbreviations = "rnbqkp";

                foreach (char c in row)
                    if (!(char.IsDigit(c) || pieceAbbreviations.Contains(c, StringComparison.OrdinalIgnoreCase)))
                        return false;
            }

            return true;
        }
    }
}
