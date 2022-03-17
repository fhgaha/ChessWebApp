using ChessWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ChessWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private static Board board;
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

            Square[,] squares = board.BoardSquares;

            List<Location> oldValidMoves = new List<Location>();
            board.ValidMoves.ForEach(loc => oldValidMoves.Add(LocationFactory.Build(loc, 0, 0)));

            board.PerformMove(board, square);
            return View("Index", board);
        }

        private static Queue<List<Square>> savedSquares = new Queue<List<Square>>();
        public IActionResult UpdateChangedSquares(string location)
        {
            File file = Array.Find(
                (File[])Enum.GetValues(typeof(File)),
                f => f.ToString() == location[0].ToString());
            int rank = int.Parse(location[1].ToString());
            Square currentSquare = board.LocationSquareMap[new Location(file, rank)];

            bool isMoveMade = board.PerformMove(board, currentSquare);

            ///use this instead of below to update whole board
            return Json(GetSquareStrings(board.LocationSquareMap.Values.ToList(), currentSquare));

            var currentSquares = new List<Square>();
            currentSquares.Add(currentSquare);
            currentSquares.AddRange(board.ValidMoves.Select(loc => board.LocationSquareMap[loc]));

            savedSquares.Enqueue(currentSquares);
            if (savedSquares.Count > 2) savedSquares.Dequeue();

            var squaresToUpdate = savedSquares.SelectMany(s => s).ToList();

            return Json(GetSquareStrings(squaresToUpdate, currentSquare));
        }

        private Dictionary<string, string> GetSquareStrings(List<Square> squares, Square square)
        {
            var squareStrings = new Dictionary<string, string>();
            string firstKey = square.Location.File.ToString() + square.Location.Rank.ToString();
            squareStrings.Add(firstKey, RenderRazorViewToString(this, "UpdateChangedSquares", square));

            foreach (Square s in squares)
            {
                Square updatedSquare = board.LocationSquareMap[s.Location];
                string key = s.Location.File.ToString() + s.Location.Rank.ToString();
                squareStrings[key] = RenderRazorViewToString(this, "UpdateChangedSquares", updatedSquare);
            }

            return squareStrings;
        }

        public static string RenderRazorViewToString(Controller controller, string viewName, object model = null)
        {
            controller.ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                IViewEngine viewEngine =
                    controller.HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as
                        ICompositeViewEngine;
                ViewEngineResult viewResult = viewEngine.FindView(controller.ControllerContext, viewName, false);

                ViewContext viewContext = new ViewContext(
                    controller.ControllerContext,
                    viewResult.View,
                    controller.ViewData,
                    controller.TempData,
                    sw,
                    new HtmlHelperOptions()
                );
                viewResult.View.RenderAsync(viewContext);
                var result = sw.GetStringBuilder().ToString();
                return result;
            }
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
