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
        private static Game game;
        private static Queue<List<Square>> savedSquares = new Queue<List<Square>>();

        public HomeController(ILogger<HomeController> logger) => _logger = logger;

        public IActionResult Index()
        {
            if (game == null) game = new();
            return View(game.Board);
        }

        public IActionResult HandleButtonClick(string location)
        {
            Location loc = game.Board.LocationSquareMap.Keys.Single(l => l.ToString() == location);
            Square square = game.Board.LocationSquareMap[loc];

            game.HandleClick(square);

            return View("Index", game.Board);
        }

        public IActionResult UpdateChangedSquares(string location)
        {
            Square currentSquare = game.Board.LocationSquareMap[LocationFactory.Parse(location)];

            bool isMoveMade = game.HandleClick(currentSquare);

            ///use this instead of below to update whole board
            return Json(GetSquareStrings(game.Board.LocationSquareMap.Values.ToList(), currentSquare));


            var currentSquares = new List<Square>();
            currentSquares.Add(currentSquare);
            currentSquares.AddRange(game.GetValidMoves().Select(loc => game.Board.LocationSquareMap[loc]));
            currentSquares.Add(game.Board.LocationSquareMap[game.Board.WhiteKing.Location]);
            currentSquares.Add(game.Board.LocationSquareMap[game.Board.BlackKing.Location]);

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
                Square updatedSquare = game.Board.LocationSquareMap[s.Location];
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


        public IActionResult CheckPromotionJSON(string location)
        {
            Location loc = LocationFactory.Parse(location);

            return Json(game.Board.LocationSquareMap[loc].CurrentPiece);
        }


        public IActionResult PromotePawn(string pieceData)
        {
            string[] data = pieceData.Split();  //"Rook white A 1"

            string className = data[0];
            string color = data[1];
            string location = string.Concat(data[2], data[3]);

            var names = new Dictionary<string, Type>
            {
                [typeof(Queen).Name] = typeof(Queen),
                [typeof(Knight).Name] = typeof(Knight),
                [typeof(Rook).Name] = typeof(Rook),
                [typeof(Bishop).Name] = typeof(Bishop)
            };

            AbstractPiece newPiece = (AbstractPiece)Activator.CreateInstance(
                names[className],
                color == "white" ? PieceColor.Light : PieceColor.Dark);

            newPiece.Location = game.Board.PawnToPromote.Location; 

            game.PromotePawn(newPiece);

            Square currentSquare = game.Board.LocationSquareMap[newPiece.Location];

            game.HandleClick(currentSquare);


            //this returns all squares. should return only part like in second part of UpdateChangedSquares method
            return Json(GetSquareStrings(game.Board.LocationSquareMap.Values.ToList(), currentSquare));

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
