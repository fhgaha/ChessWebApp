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
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            if (board == null) board = new Board();
            return View(board.BoardSquares);
        }

        public IActionResult HandleButtonClick(string location)
        {
            Location loc = board.LocationSquareMap.Keys.Single(l => l.ToString() == location);
            Square square = board.LocationSquareMap[loc];
            AbstractPiece piece = square.CurrentPiece;

            if (piece != null)  //clicked on piece
            {
                fromSquare = square;
            }
            else if (fromSquare.CurrentPiece != null)    //clicked on empty square
            {
                fromSquare.CurrentPiece.MakeMove(square);
                fromSquare.Reset();
                board.LocationSquareMap[fromSquare.Location].Reset();
            }




            return View("Index", board.BoardSquares);
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
