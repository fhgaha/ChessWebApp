using ChessWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ChessWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private static Square fromSquare;
        private static Board board;
        private static bool IsWhitesMove = true;
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            if (board == null) board = new Board();
            return View(board);
        }

        public IActionResult HandleButtonClick(string location)
        {
            Location loc = board.LocationSquareMap.Keys.Single(l => l.ToString() == location);
            Square square = board.LocationSquareMap[loc];

            Func<bool> FromSquareIsSelected = () => fromSquare != null && fromSquare.CurrentPiece != null;
            Func<bool> SquareIsEmpty = () => square.CurrentPiece == null;

            Func<bool> SelectedAndTargetPieceAreTheSamePiece = ()
                => fromSquare == square && fromSquare.CurrentPiece != null;

            Func<bool> SelectedAndTargetPieceColorsAreTheSame = ()
                => fromSquare.CurrentPiece.PieceColor == square.CurrentPiece.PieceColor;

            Func<bool> MoveOrderIsWrong = ()
                 => IsWhitesMove && fromSquare.CurrentPiece.PieceColor != PieceColor.Light
                || !IsWhitesMove && fromSquare.CurrentPiece.PieceColor != PieceColor.Dark;

            if (!FromSquareIsSelected())
            {
                fromSquare = square;

                if (fromSquare.CurrentPiece != null && !MoveOrderIsWrong())
                    board.UpdateValidSquares(fromSquare.CurrentPiece);
            }
            else
            {
                if (MoveOrderIsWrong())
                {
                    fromSquare = null;
                }
                else
                {
                    board.UpdateValidSquares(fromSquare.CurrentPiece);

                    if (board.ValidMoves.Contains(square.Location))
                    {
                        fromSquare.CurrentPiece.MakeMove(square);
                        IsWhitesMove = !IsWhitesMove;
                    }
                    fromSquare = null;
                }
                board.ValidMoves.Clear();
            }
            return View("Index", board);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
