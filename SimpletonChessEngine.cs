using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpletonChessEngine
{
    public class SimpletonChessEngine
    {
        private readonly GameState gameState;
        private readonly MoveGenerator moveGenerator;
        private readonly Evaluator evaluator;
        private readonly SearchAlgorithm searchAlgorithm;

        public SimpletonChessEngine()
        {
            gameState = new GameState();
            moveGenerator = new MoveGenerator();
            evaluator = new Evaluator();
            searchAlgorithm = new SearchAlgorithm(evaluator);
        }

        // WinBoard/XBoard protokol
        public void RunWinBoard()
        {
            var winboardHandler = new WinBoardHandler(this);
            winboardHandler.Run();
        }

        // UCI protokol (za druge GUI-jeve)
        public void RunUCI()
        {
            var uciHandler = new UCIHandler(this);
            uciHandler.Run();
        }

        // Lichess Bot API
        public async Task RunLichessBot()
        {
            var lichessBot = new LichessBot(this);
            await lichessBot.StartAsync();
        }

        // Auto-detektovanje protokola
        public void RunAutoDetect()
        {
            Console.WriteLine("Chess Engine Ready - Waiting for protocol detection...");

            string firstCommand = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(firstCommand))
                return;

            switch (firstCommand.ToLower())
            {
                case "uci":
                    var uciHandler = new UCIHandler(this);
                    uciHandler.HandleCommand(firstCommand);
                    uciHandler.Run();
                    break;
                case "xboard":
                case "protover 2":
                    var winboardHandler = new WinBoardHandler(this);
                    winboardHandler.HandleCommand(firstCommand);
                    winboardHandler.Run();
                    break;
                default:
                    // Pokušaj WinBoard kao default
                    var defaultHandler = new WinBoardHandler(this);
                    defaultHandler.HandleCommand(firstCommand);
                    defaultHandler.Run();
                    break;
            }
        }

        // Core engine methods
        public string GetBestMove(string position = null)
        {
            try
            {
                Console.Error.WriteLine("[DEBUG] GetBestMove called");

                if (!string.IsNullOrEmpty(position))
                {
                    gameState.SetPosition(position);
                }

                Console.Error.WriteLine("[DEBUG] Calling FindBestMove...");
                var bestMove = searchAlgorithm.FindBestMove(gameState, depthLimit: 3);
                Console.Error.WriteLine($"[DEBUG] FindBestMove returned: {bestMove?.ToString() ?? "null"}");

                if (bestMove == null)
                {
                    Console.Error.WriteLine("[DEBUG] bestMove is null, using fallback");
                    return "e2e4";
                }

                string result = bestMove.ToString();
                Console.Error.WriteLine($"[DEBUG] Move string: '{result}'");

                if (string.IsNullOrEmpty(result))
                {
                    Console.Error.WriteLine("[DEBUG] Empty result, using fallback");
                    return "e2e4";
                }

                // NE dodavaj engine move u state ovde - to će GUI uraditi u sledećem position command
                Console.Error.WriteLine($"[DEBUG] Returning: '{result}'");
                return result;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[DEBUG] Exception in GetBestMove: {ex.Message}");
                Console.Error.WriteLine($"[DEBUG] StackTrace: {ex.StackTrace}");
                return "e2e4";
            }
        }

        public void MakeMove(string move)
        {
            var parsedMove = Move.Parse(move);
            if (gameState.IsLegalMove(parsedMove))
            {
                gameState.MakeMove(parsedMove);
            }
        }

        public void NewGame()
        {
            gameState.Reset();
        }

        public void SetPosition(string fen)
        {
            gameState.SetPosition(fen);
        }

        public bool IsGameOver()
        {
            return gameState.IsCheckmate() || gameState.IsStalemate() || gameState.IsDraw();
        }
    }

    // Dummy classes - implementiraj prema potrebi
   
    public class MoveGenerator
    {
        // Generiši sve legalne poteze
    }

    public class Evaluator
    {
        public int Evaluate(GameState position)
        {
            // Evaluacija pozicije (material, pozicija, itd.)
            return 0;
        }
    }

    public class SearchAlgorithm
    {
        private readonly Evaluator evaluator;

        public SearchAlgorithm(Evaluator evaluator)
        {
            this.evaluator = evaluator;
        }

        public Move FindBestMove(GameState position, int depthLimit)
        {
            try
            {
                Console.Error.WriteLine("[DEBUG] FindBestMove starting...");

                int moveCount = position.GetMoveCount();
                bool isWhiteToMove = (moveCount % 2 == 0);

                Console.Error.WriteLine($"[DEBUG] Move count: {moveCount}, White to move: {isWhiteToMove}");

                // Različiti potezi za beli i crni
                if (isWhiteToMove)
                {
                    // Beli potezi
                    string[] moves = { "e2e4", "d2d4", "g1f3", "b1c3" };
                    Random rand = new Random();
                    string move = moves[rand.Next(moves.Length)];
                    Console.Error.WriteLine($"[DEBUG] White move: {move}");
                    return Move.Parse(move);
                }
                else
                {
                    // Crni potezi - odgovori na beli potez
                    string[] moves = { "e7e5", "d7d5", "g8f6", "b8c6" };
                    Random rand = new Random();
                    string move = moves[rand.Next(moves.Length)];
                    Console.Error.WriteLine($"[DEBUG] Black move: {move}");
                    return Move.Parse(move);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[DEBUG] Exception: {ex.Message}");
                return Move.Parse("e2e4");
            }
        }
    }

}
