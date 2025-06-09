using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpletonChessEngine
{
    public class SimpletonChessEngine : IChessEngine
    {
        public volatile bool shouldStop = false;
        public bool ShouldStop
        {
            get => shouldStop;
            set => shouldStop = value;
        }

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

        public string GetBestMove(string position = null)
        {
            Console.WriteLine("info string DEBUG TEST - GetBestMove called");
            Console.WriteLine("info string About to show board state...");


            try
            {
                Console.WriteLine("info string ChessEngine.GetBestMove called");
                if (!string.IsNullOrEmpty(position))
                {
                    gameState.SetPosition(position);
                }


                var bestMove = searchAlgorithm.FindBestMove(gameState, depthLimit: 3, shouldStop: () => ShouldStop);

                if (bestMove == null)
                {
                    Console.WriteLine("info string bestMove is null, using fallback");
                    return "e2e4";
                }

                string result = bestMove.ToString();
                Console.WriteLine($"info string GetBestMove returning: {result}");

                if (string.IsNullOrEmpty(result))
                {
                    Console.WriteLine("info string Empty result, using fallback");
                    return "e2e4";
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"info string Exception in GetBestMove: {ex.Message}");
                Console.WriteLine($"info string StackTrace: {ex.StackTrace}");
                return "e2e4";
            }
        }


        public void MakeMove(string move)
        {
            var parsedMove = Move.Parse(move);
            if (parsedMove != null)
            {
                // DIREKTNO PRIMENI - GUI šalje samo legalne poteze!
                gameState.MakeMove(parsedMove);
            }
            else
            {
                Console.WriteLine($"info string ERROR: Failed to parse move '{move}'");
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

        public void SetPosition(string[] moves)
        {
            NewGame(); // reset na početnu poziciju
            foreach (string move in moves)
            {
                MakeMove(move);
            }
        }

        public bool IsGameOver()
        {
            return gameState.IsCheckmate() || gameState.IsStalemate() || gameState.IsDraw();
        }

    }
}
