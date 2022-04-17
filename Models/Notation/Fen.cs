using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessWebApp.Models.Notation
{
    public class Fen
    {
        private int halfmoveCount = 0;
        public string Default { get; set; } = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
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

        public string Get(Board board)
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

            builder.Append(" " + GetWhosMoveIsNext(board));
            builder.Append(" " + GetCastlingLetters(board));
            builder.Append(" " + GetEnPassantCandidate(board));
            builder.Append(" " + Get50MoveDrawCount(board));
            builder.Append(" " + GetFullmovesCount(board));

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
            bool lastMovePieceColorIsWhite = board.LastMove.Item2.CurrentPiece.PieceColor == PieceColor.Light;
            return lastMovePieceColorIsWhite ? " b" : " w";
        }

        private string GetCastlingLetters(Board board)
        {
            string result = "";

            if (board.WhiteKing.isAbleToCastle)
            {
                //short castling
                if (board.LocationSquareMap[new Location(File.H, 1)].CurrentPiece is Rook rookH && rookH is not null
                    && rookH.IsFirstMove)
                    result += "K";

                //long castling
                if (board.LocationSquareMap[new Location(File.A, 1)].CurrentPiece is Rook rookA && rookA is not null
                    && rookA.IsFirstMove)
                    result += "Q";
            }

            if (board.BlackKing.isAbleToCastle)
            {
                //short castling
                if (board.LocationSquareMap[new Location(File.H, 8)].CurrentPiece is Rook rookH && rookH is not null
                    && rookH.IsFirstMove)
                    result += "k";

                //long castling
                if (board.LocationSquareMap[new Location(File.A, 8)].CurrentPiece is Rook rookA && rookA is not null
                    && rookA.IsFirstMove)
                    result += "q";
            }

            return result == "" ? result + "-" : result;
        }

        private string GetEnPassantCandidate(Board board)
            => LocationFactory.Parse(board.PawnToBeTakenEnPassant.Location);

        private string Get50MoveDrawCount(Board board)
        {
            if (board.LastMove is null) return "-";

            //NB!
            //what to do if pawn promoted? should check fen after moving pawn to the last rank and before 
            //changing pawn to a piece
            if (board.LastMove.Item2.CurrentPiece is Pawn || board.PieceCapturedOnLastMove != null)
                halfmoveCount = 0;
            else
                halfmoveCount++;

            return halfmoveCount.ToString();
        }

        private string GetFullmovesCount(Board board) => (board.PerformedMoves.Count / 2).ToString();
    }
}
