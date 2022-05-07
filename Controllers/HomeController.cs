using ChessWebApp.Models;
using ChessWebApp.Models.Notation;
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

namespace ChessWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private static Game game;
        private static Queue<List<Square>> savedSquares = new Queue<List<Square>>();
        private Fen fen = new();

        public HomeController(ILogger<HomeController> logger) => _logger = logger;

        public IActionResult Index()
        {
            if (game == null) game = new();
            return View(game.Board);
        }

        public string GetFen() => fen.Parse(game.Board);

        public string GetPlayer() => game.PlayerToMove.ToString();

        public IActionResult SetFenJSON(string fenCode)
        {
            if (game.Fen.ValidateInput(fenCode) == false) return RedirectToAction("UpdateChangedSquaresJSON", null);

            game = new(fenCode);
            savedSquares.Clear();

            //should update fen input
            //and whole board
            return Json(GetSquareStrings(game.Board.LocationSquareMap.Values.ToList(),
                game.Board.LocationSquareMap[new Location(ChessWebApp.File.A, 1)]));
        }

        public IActionResult RestartGame()
        {
            game = null;
            savedSquares.Clear();
            return RedirectToAction("Index");
        }

        public IActionResult HandleButtonClick(string location)
        {
            Location loc = game.Board.LocationSquareMap.Keys.Single(l => l.ToString() == location);
            Square square = game.Board.LocationSquareMap[loc];

            game.HandleClick(square);

            return View("Index", game.Board);
        }

        public IActionResult UpdateChangedSquaresJSON(string location)
        {
            Location loc = LocationFactory.Parse(location);
            if (loc is null) return RedirectToAction("Index");

            Square currentSquare = game.Board.LocationSquareMap[loc];

            game.HandleClick(currentSquare);

            UpdateSquaresToDislayColored();

            ///this return updates all squares
            return Json(GetSquareStrings(game.Board.LocationSquareMap.Values.ToList(), currentSquare));

            ///this code section updates only changed squares
            savedSquares.Enqueue(GetChangedSquares(currentSquare));
            if (savedSquares.Count > 2) savedSquares.Dequeue();

            var squaresToUpdate = savedSquares.SelectMany(s => s).ToList();

            return Json(GetSquareStrings(squaresToUpdate, currentSquare));
        }

        private static void UpdateSquaresToDislayColored()
        {
            //set all moves as not valid then new valid moves as valid
            game.SetAllSquaresNotValid();
            game.SetProperSquaresAsValid();
        }

        private List<Square> GetChangedSquares(Square square)
        {
            var changedSquares = new List<Square>();
            changedSquares.Add(square);
            changedSquares.AddRange(game.GetValidMoves().Select(loc => game.Board.LocationSquareMap[loc]));
            changedSquares.Add(game.Board.LocationSquareMap[game.Board.WhiteKing.Location]);
            changedSquares.Add(game.Board.LocationSquareMap[game.Board.BlackKing.Location]);


            //in case of castling update square where castling rook was
            //or in case of en passant update where taken pawn was
            if (game.MoveManager.AdditionalSquareToUpdate is Square sq && sq is not null)
            {
                changedSquares.Add(sq);
                game.MoveManager.AdditionalSquareToUpdate = null;
            }

            return changedSquares;
        }

        private Dictionary<string, string> GetSquareStrings(List<Square> squares, Square square)
        {
            var squareStrings = new Dictionary<string, string>();

            //add to result clicked square
            if (square is not null)
            {
                string firstKey = square.Location.File.ToString() + square.Location.Rank.ToString();
                squareStrings.Add(firstKey, RenderRazorViewToString(this, "UpdateChangedSquares", square));
            }

            //add to result updated squares
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
                IViewEngine viewEngine = controller.HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) 
                    as ICompositeViewEngine;
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

        public IActionResult PromotePawnJSON(string pieceData)
        {
            string[] data = pieceData.Split();  //"Rook white A 1"

            string className = data[0];
            string color = data[1];
            string location = string.Concat(data[2], data[3]);
            Type type = PieceFactory.AbstractPieceSubclasses[className];

            AbstractPiece newPiece = (AbstractPiece)Activator.CreateInstance(
                type,
                color == "white" ? PieceColor.Light : PieceColor.Dark);

            newPiece.Location = game.Board.PawnToPromote.Location;

            game.PromotePawn(newPiece);

            Square currentSquare = game.Board.LocationSquareMap[newPiece.Location];

            UpdateSquaresToDislayColored();

            return UpdateChangedSquaresJSON(newPiece.Location.File.ToString() + newPiece.Location.Rank.ToString());
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

        public IActionResult UndoMove()
        {
            game.UndoMove();
            return Json(GetSquareStrings(game.Board.LocationSquareMap.Values.ToList(), null));
        }

        //need redo move method
    }


}
