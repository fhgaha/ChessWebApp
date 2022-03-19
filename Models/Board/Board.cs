using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessWebApp
{
    public class Board
    {
        public bool GameIsOver = false;
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

            var pieces =
                PieceFactory.GetStandartPiecePositions();
            //PieceFactory.GetTwoKings();
            //PieceFactory.GetCastlingSetup();

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


        public bool HandleClick(Board board, Square square)
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
                if (SquareIsEmpty())
                    FromSquare = null;
                else if (MoveOrderIsWrong(square))
                    FromSquare = null;
                else
                {
                    FromSquare = square;
                    board.UpdateValidSquares(square);
                }
            }
            else if (FromSquareIsSelected())
            {
                if (MoveOrderIsWrong(FromSquare))
                    FromSquare = null;
                else if (!SquareIsEmpty() && SelectedAndTargetPieceAreTheSamePiece())
                    FromSquare = null;
                else if (!SquareIsEmpty() && SelectedAndTargetPieceAreTheSameColor())
                {
                    FromSquare = square;
                    board.UpdateValidSquares(FromSquare);
                }
                else if (ValidMoves.Contains(square.Location))  //selected and moving allowed
                {
                    isMovePerformed = MakeMove(board, square);
                    FromSquare = null;
                }
                else
                    FromSquare = null;
            }
            else
                FromSquare = null;

            WhiteKing.UpdateIsUnderCheck(board.LocationSquareMap[WhiteKing.Location]);
            BlackKing.UpdateIsUnderCheck(board.LocationSquareMap[BlackKing.Location]);

            return isMovePerformed;
        }


        private bool MakeMove(Board board, Square square)
        {
            if (square.CurrentPiece != null) RemoveAttackerFromAllAttackedByPieceOnSquareLists(board, square);
            RemoveAttackerFromAllAttackedByPieceOnSquareLists(board, FromSquare);

            FromSquare.MovePiece(square);

            //castling
            if (square.CurrentPiece is King && Math.Abs(FromSquare.Location.File - square.Location.File) == 2)
                HandleCastling(board, square);

            LocationSquareMap.Values.ToList().ForEach(sq => UpdateSquaresAttackedByPiece(this, sq));

            //MovingPieceBesidesTheseLocsDiscoversCheck(square).ForEach(loc => board.LocationSquareMap[loc].IsValid = true);

            IsWhitesMove = !IsWhitesMove;

            board.PerformedMoves.Add(Tuple.Create(fromSquare, square));
            return true;
        }

        private void HandleCastling(Board board, Square square)
        {
            Square FromRookSquare = null;
            Square ToRookSquare = null;
            if (square.Location == new Location(File.C, 1))
            {
                FromRookSquare = board.LocationSquareMap[new Location(File.A, 1)];
                ToRookSquare = board.LocationSquareMap[new Location(File.D, 1)];
            }
            else if (square.Location == new Location(File.G, 1))
            {
                FromRookSquare = board.LocationSquareMap[new Location(File.H, 1)];
                ToRookSquare = board.LocationSquareMap[new Location(File.F, 1)];
            }
            else if (square.Location == new Location(File.C, 8))
            {
                FromRookSquare = board.LocationSquareMap[new Location(File.A, 8)];
                ToRookSquare = board.LocationSquareMap[new Location(File.D, 8)];
            }
            else if (square.Location == new Location(File.G, 8))
            {
                FromRookSquare = board.LocationSquareMap[new Location(File.H, 8)];
                ToRookSquare = board.LocationSquareMap[new Location(File.F, 8)];
            }

            RemoveAttackerFromAllAttackedByPieceOnSquareLists(board, FromRookSquare);
            FromRookSquare.MovePiece(ToRookSquare);
            UpdateSquaresAttackedByPiece(board, ToRookSquare);
        }

        public void UpdateValidSquares(Square square)
        {
            var legalMoves = MovingPieceBesidesTheseLocsDiscoversCheck(square);

            SetAllSquaresNotValid();
            legalMoves.ForEach(loc => LocationSquareMap[loc].IsValid = true);
            ValidMoves = legalMoves;
        }

        private List<Location> MovingPieceBesidesTheseLocsDiscoversCheck(Square defender)
        {
            var moves = defender.CurrentPiece.GetValidMoves(this, defender);
            King king = IsWhitesMove ? WhiteKing : BlackKing;

            moves = FilterMovesToPreventCheck(this, moves, defender.CurrentPiece, king);

            return moves;
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

                MakeFakeMove(fDefenderSquare, squareCandidate, defendingMoves, candidate, fBoard,
                    (King)fKingSquare.CurrentPiece);
            }
            return defendingMoves;
        }

        private void MakeFakeMove(Square from, Square to, List<Location> defendingMoves,
            Location candidate, Board fBoard, King fKing)
        {
            //if candidate contains attacker, after moving a defender the attaker would be taken.
            //update king's square for every other attacker and check if king is still attacked

            Square fKingSquare = fBoard.LocationSquareMap[fKing.Location];

            if (to.IsOccupied && fKingSquare.AttackedByPiecesOnSquares.Contains(to))
            {
                fBoard.RemoveAttackerFromAllAttackedByPieceOnSquareLists(fBoard, from);
                from.MovePiece(to);
                fBoard.LocationSquareMap.Values.ToList().ForEach(sq => UpdateSquaresAttackedByPiece(fBoard, sq));

                fKingSquare.AttackedByPiecesOnSquares.Where(p => p.Location != candidate).ToList()
                    .ForEach(attacker => UpdateSquaresAttackedByPiece(fBoard, attacker));
            }
            else
            {
                fBoard.RemoveAttackerFromAllAttackedByPieceOnSquareLists(fBoard, from);
                from.MovePiece(to);
                fBoard.LocationSquareMap.Values.ToList().ForEach(sq => UpdateSquaresAttackedByPiece(fBoard, sq));

                //need to update all attacker square lists?
                fBoard.LocationSquareMap.Values.Where(sq => sq.CurrentPiece != null)
                    .ToList()
                    .ForEach(sq => fBoard.UpdateSquaresAttackedByPiece(fBoard, sq));

                List<Square> attackers = new();
                attackers.AddRange(fKingSquare.AttackedByPiecesOnSquares);
                attackers.ForEach(attacker => UpdateSquaresAttackedByPiece(fBoard, attacker));
            }

            fKingSquare = fBoard.LocationSquareMap[fKing.Location];
            if (fKing.UpdateIsUnderCheck(fKingSquare) == false)
                defendingMoves.Add(candidate);
        }

        private void UpdateSquaresAttackedByPiece(Board board, Square attacker)
        {
            if (attacker.CurrentPiece == null) return;

            RemoveAttackerFromAllAttackedByPieceOnSquareLists(board, attacker);

            var attackedLocs = attacker.CurrentPiece.GetLocationsAttackedByPiece(board, attacker);

            attackedLocs.ForEach(loc => board.LocationSquareMap[loc].AttackedByPiecesOnSquares.Add(attacker));
        }

        private void RemoveAttackerFromAllAttackedByPieceOnSquareLists(Board board, Square attacker) =>
            board.LocationSquareMap.Values
                .Where(sq => sq.AttackedByPiecesOnSquares.Contains(attacker)).ToList()
                .ForEach(sq => sq.AttackedByPiecesOnSquares.Remove(attacker));
    }
}
