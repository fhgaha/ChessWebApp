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

            MoveHandler.PerformMove(board, square);
            return View("Index", board);
        }

        public IActionResult UpdateChangedSquares(string location)
        {
            File file = ChessWebApp.File.A;

            foreach (File f in Enum.GetValues(typeof(File)))
                if (f.ToString() == location[0].ToString())
                {
                    file = f;
                    break;
                }

            int rank = int.Parse(location[1].ToString());
            Square square = board.LocationSquareMap[new Location(file, rank)];

            MoveHandler.PerformMove(board, square);

            return PartialView(square);
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


    }
}
