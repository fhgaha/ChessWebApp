using ChessWebApp.Models.MoveModel;
using ChessWebApp.Models.Notation;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;

namespace ChessWebApp.Models.Players
{
    public class MachinePlayer : Player
    {
        public int Score { get; set; }
        public List<AbstractPiece> CapturedPieces { get; set; }
        private Engine.MyEngine engine ;
        private Fen fen = new();
        //private IStockfish stockfish;


        public MachinePlayer()
        {
            CapturedPieces = new();
            engine = new Engine.MyEngine();
            
            //string winPath = @"Models\Engine\stockfish_win_20090216_x64_avx2.exe";
            //cant find linux file
            //string linuxPath = @"Models\Engine\stockfish_linux_20090216_x64_avx2";
            //Settings settings = new(skillLevel: 20);
            //stockfish = new Stockfish.NET.Stockfish(winPath, depth: 3, settings);
        }

        //public bool TryMakeMove(Game game, MoveManager moveManager, Square square)
        //{
        //    Board board = game.Board;

        //    var positionFen = fen.Parse(board);

        //    stockfish.SetFenPosition(positionFen);
        //    string bestMoveString = stockfish.GetBestMove();

        //    //string bestMoveString = GetFromLichess(positionFen);

        //    Move bestMove = Move.Parse(bestMoveString);

        //    bool isMovePerformed = moveManager.MakeMove(
        //        board,
        //        board.LocationSquareMap[bestMove.From],
        //        board.LocationSquareMap[bestMove.To]);

        //    return isMovePerformed;
        //}

        private string GetFromLichess(string fen)
        {
            var baseUrl = "https://lichess.org/api/cloud-eval";

            var request = WebRequest.Create(baseUrl + "?fen=" + fen);
            request.Method = "GET";

            using var webResponse = request.GetResponse();
            using var webStream = webResponse.GetResponseStream();

            using var reader = new StreamReader(webStream);
            var data = reader.ReadToEnd();

            string moves = "";

            using (JsonDocument document = JsonDocument.Parse(data))
            {
                JsonElement root = document.RootElement;
                var pvs = root.GetProperty("pvs");
                foreach (JsonElement element in pvs.EnumerateArray())
                {
                    if (element.TryGetProperty("moves", out JsonElement movesElement))
                    {
                        moves = movesElement.GetString();
                        break;
                    }
                }
            }

            string move = moves.Split(' ').First();

            return move;
        }

        public bool TryMakeMove(Game game, MoveManager moveManager, Square square)
        {
            bool isMovePerformed = false;
            Board board = game.Board;

            Move bestMove = engine.GetBestMove(board);

            isMovePerformed = moveManager.MakeMove(
                board,
                board.LocationSquareMap[bestMove.From],
                board.LocationSquareMap[bestMove.To]);

            return isMovePerformed;
        }
    }
}
