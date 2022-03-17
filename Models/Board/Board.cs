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
        public List<Location> ValidMoves { get; private set; }
        public King WhiteKing { get; private set; }
        public King BlackKing { get; private set; }
        public string message = "";


        //from moveHandler
        public List<Tuple<Square, Square>> completedMoves = new List<Tuple<Square, Square>>();
        public bool IsWhitesMove = true;
        private Square fromSquare;
        private Square FromSquare
        {
            get => fromSquare;
            set
            {
                if (value == null) ValidMoves.Clear();
                LocationSquareMap.Values.ToList().ForEach(sq => sq.IsValid = false);
                fromSquare = value;
            }
        }


        public Board()
        {
            BoardSquares = new Square[Constants.BoardLength, Constants.BoardLength];
            LightPieces = new List<AbstractPiece>();
            DarkPieces = new List<AbstractPiece>();
            ValidMoves = new List<Location>();
            LocationSquareMap = new Dictionary<Location, Square>();

            var pieces = PieceFactory.GetStandartPiecePositions();
            //var pieces = PieceFactory.GetTwoKings();

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

                        if (piece.PieceColor == PieceColor.Light) LightPieces.Add(piece);
                        else DarkPieces.Add(piece);
                    }
                    BoardSquares[i, column] = newSquare;
                    LocationSquareMap.Add(newSquare.Location, newSquare);
                    currentColor = currentColor == SquareColor.Light ? SquareColor.Dark : SquareColor.Light;
                    column++;
                }
            }

            WhiteKing = (King)LightPieces.Find(piece => piece is King);
            BlackKing = (King)DarkPieces.Find(piece => piece is King);
        }


        public void SetAllSquaresNotValid()
        {
            LocationSquareMap.Values.ToList().ForEach(sq => sq.IsValid = false);
        }


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


        //fromMoveHandler

        public bool PerformMove(Board board, Square square)
        {
            bool isMovePerformed = true;

            Func<bool> FromSquareIsSelected = () => FromSquare != null && FromSquare.CurrentPiece != null;
            Func<bool> SquareIsEmpty = () => square.CurrentPiece == null;

            Func<bool> SelectedAndTargetPieceAreTheSamePiece = ()
                => FromSquare == square && FromSquare.CurrentPiece != null;

            Func<bool> SelectedAndTargetPieceAreTheSameColor = ()
                => FromSquare.CurrentPiece.PieceColor == square.CurrentPiece.PieceColor;

            Func<Square, bool> MoveOrderIsWrong = sq
                 => IsWhitesMove && sq.CurrentPiece.PieceColor != PieceColor.Light
                || !IsWhitesMove && sq.CurrentPiece.PieceColor != PieceColor.Dark;


            if (!FromSquareIsSelected())
            {
                if (!SquareIsEmpty() && !MoveOrderIsWrong(square))
                {
                    FromSquare = square;

                    if (FromSquare.CurrentPiece != null && !MoveOrderIsWrong(FromSquare))
                        board.UpdateValidSquares(FromSquare);
                }
            }
            else if (MoveOrderIsWrong(FromSquare))
            {
                FromSquare = null;
            }
            else if (!SquareIsEmpty() && SelectedAndTargetPieceAreTheSamePiece())
            {
                FromSquare = null;
            }
            else if (!SquareIsEmpty() && SelectedAndTargetPieceAreTheSameColor())
            {
                FromSquare = square;
                board.UpdateValidSquares(FromSquare);
            }
            else
            {   //making a move

                //board.UpdateValidSquares(FromSquare.CurrentPiece);

                if (board.ValidMoves.Contains(square.Location))
                {
                    FromSquare.MovePiece(square);
                    UpdateSquaresAttackedByPiece(board, square);

                    IsWhitesMove = !IsWhitesMove;
                    isMovePerformed = true;

                    board.PerformedMoves.Add(Tuple.Create(fromSquare, square));
                }
                FromSquare = null;
            }

            return isMovePerformed;
        }


        public void UpdateValidSquares(Square square)
        {
            var moves = square.CurrentPiece.GetValidMoves(this, square);

            King king = IsWhitesMove ? WhiteKing : BlackKing;

            Square kingSquare = LocationSquareMap[king.Location];

            if (IsReal && king.IsUnderCheck(kingSquare))
            {
                moves = FilterMovesToPreventCheck(this, moves, square.CurrentPiece, king);
                SetAllSquaresNotValid();

                if (moves.Count == 0)
                {
                    message = king + " is checkmated!";
                    return;
                }

                foreach (var item in moves)
                    LocationSquareMap[item].IsValid = true;
            }

            ValidMoves = moves;
        }


        public List<Location> FilterMovesToPreventCheck(Board originalBoard, List<Location> candidates,
            AbstractPiece defender, King king)
        {
            List<Location> defendingMoves = new List<Location>();

            foreach (Location candidate in candidates)
            {
                Board fBoard = new Board() { IsReal = false };

                originalBoard.LocationSquareMap.Keys.ToList()
                    .ForEach(loc => fBoard.LocationSquareMap[loc] = new Square(originalBoard.LocationSquareMap[loc]));

                //copy attacked by pieces on squares list for each square
                foreach (Location loc in fBoard.LocationSquareMap.Keys)
                {
                    var originalSquareAttackersList = originalBoard.LocationSquareMap[loc].AttackedByPiecesOnSquares;
                    var fakeSquareAttackersList = fBoard.LocationSquareMap[loc].AttackedByPiecesOnSquares;

                    fBoard.LocationSquareMap[loc].CopyAttackedByPiecesOnSquares(fBoard, originalSquareAttackersList);
                }

                Square squareCandidate = fBoard.LocationSquareMap[candidate];

                Square fDefenderSquare = fBoard.LocationSquareMap[defender.Location];   

                Square fKingSquare = fBoard.LocationSquareMap[king.Location];

                var originalkingAttackersList = originalBoard.LocationSquareMap[king.Location].AttackedByPiecesOnSquares;
                fKingSquare.CopyAttackedByPiecesOnSquares(fBoard, originalkingAttackersList);

                MakeFakeMove(fDefenderSquare, squareCandidate, defendingMoves, candidate, fBoard, fKingSquare);
                //fBoard.SetAllSquaresNotValid();
            }
            return defendingMoves;
        }

        private static void MakeFakeMove(Square from, Square to, List<Location> defendingMoves,
            Location candidate, Board fBoard, Square fKingSquare)
        {
            //if candidate contains attacker, after moving a defender the attaker would be taken.
            //update king's square for every other attacker and check if king is still attacked

            if (to.IsOccupied && fKingSquare.AttackedByPiecesOnSquares.Contains(to))
            {
                from.MovePiece(to);

                fKingSquare.AttackedByPiecesOnSquares.Where(p => p.Location != candidate).ToList()
                    .ForEach(attacker => UpdateSquaresAttackedByPiece(fBoard, attacker));
            }
            else
            {
                from.MovePiece(to);

                List<Square> attackers = new();
                attackers.AddRange(fKingSquare.AttackedByPiecesOnSquares);
                attackers.ForEach(attacker => UpdateSquaresAttackedByPiece(fBoard, attacker));
            }

            if (((King)fKingSquare.CurrentPiece).IsUnderCheck(fKingSquare) == false)
                defendingMoves.Add(candidate);

            //to.MovePiece(from);
        }

        private static void UpdateSquaresAttackedByPiece(Board board, Square attacker)
        {
            foreach (Square sq in board.LocationSquareMap.Values)
                if (sq.AttackedByPiecesOnSquares.Contains(attacker))
                    sq.AttackedByPiecesOnSquares.Remove(attacker);

            var attackedLocs = attacker.CurrentPiece.GetLocationsAttackedByPiece(board, attacker);

            attackedLocs.ForEach(loc => board.LocationSquareMap[loc].AttackedByPiecesOnSquares.Add(attacker));
        }
    }
}
