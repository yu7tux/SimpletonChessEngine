using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpletonChessEngine
{
    public class WinBoardHandler
    {
        private readonly SimpletonChessEngine engine;
        private bool forceMode = false;
        private bool engineSide = false; // true ako engine igra crne
        private bool gameInProgress = false;

        public WinBoardHandler(SimpletonChessEngine engine)
        {
            this.engine = engine;
        }

        public void Run()
        {
            while (true)
            {
                try
                {
                    string input = Console.ReadLine();
                    if (string.IsNullOrEmpty(input))
                        continue;

                    HandleCommand(input.Trim());
                }
                catch (Exception ex)
                {
                    // Log error but continue running
                    Console.Error.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        public void HandleCommand(string command)
        {
            string[] parts = command.Split(' ');
            string cmd = parts[0].ToLower();

            switch (cmd)
            {
                case "xboard":
                    // WinBoard initialization
                    break;

                case "protover":
                    HandleProtocolVersion(parts);
                    break;

                case "new":
                    HandleNewGame();
                    break;

                case "force":
                    forceMode = true;
                    break;

                case "go":
                    HandleGo();
                    break;

                case "white":
                    engineSide = false;
                    break;

                case "black":
                    engineSide = true;
                    break;

                case "time":
                    if (parts.Length > 1 && int.TryParse(parts[1], out int timeLeft))
                    {
                        // Postavi vreme za engine (u centisekundama)
                    }
                    break;

                case "otim":
                    if (parts.Length > 1 && int.TryParse(parts[1], out int oppTime))
                    {
                        // Postavi vreme za protivnika
                    }
                    break;

                case "usermove":
                    if (parts.Length > 1)
                    {
                        HandleUserMove(parts[1]);
                    }
                    break;

                case "ping":
                    if (parts.Length > 1)
                    {
                        Console.WriteLine($"pong {parts[1]}");
                    }
                    break;

                case "quit":
                    Environment.Exit(0);
                    break;

                case "undo":
                    // Implementiraj undo
                    break;

                case "remove":
                    // Implementiraj remove (undo 2 moves)
                    break;

                case "result":
                    HandleGameResult(command);
                    break;

                case "setboard":
                    if (parts.Length > 1)
                    {
                        string fen = string.Join(" ", parts, 1, parts.Length - 1);
                        engine.SetPosition(fen);
                    }
                    break;

                default:
                    // Možda je običan potez (bez "usermove" prefiksa)
                    if (IsValidMoveFormat(cmd))
                    {
                        HandleUserMove(cmd);
                    }
                    break;
            }
        }

        private void HandleProtocolVersion(string[] parts)
        {
            if (parts.Length > 1 && parts[1] == "2")
            {
                // Pošalji feature komande
                Console.WriteLine("feature ping=1 setboard=1 playother=1 san=0 usermove=1");
                Console.WriteLine("feature time=1 draw=1 sigint=0 sigterm=0");
                Console.WriteLine("feature myname=\"MojChessEngine 1.0\"");
                Console.WriteLine("feature done=1");
            }
        }

        private void HandleNewGame()
        {
            engine.NewGame();
            forceMode = false;
            engineSide = false;
            gameInProgress = true;
        }

        private void HandleGo()
        {
            forceMode = false;
            if (gameInProgress)
            {
                MakeEngineMove();
            }
        }

        private void HandleUserMove(string move)
        {
            try
            {
                engine.MakeMove(move);

                if (!forceMode && gameInProgress && !engine.IsGameOver())
                {
                    // Engine treba da odigra potez
                    Thread.Sleep(100); // Kratka pauza za stabilnost
                    MakeEngineMove();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Illegal move: {move}");
            }
        }

        private void MakeEngineMove()
        {
            try
            {
                string bestMove = engine.GetBestMove();
                if (!string.IsNullOrEmpty(bestMove) && bestMove != "null")
                {
                    Console.WriteLine($"move {bestMove}");
                    engine.MakeMove(bestMove);
                }
                else
                {
                    // Nema legalnih poteza
                    if (engine.IsGameOver())
                    {
                        gameInProgress = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Engine move error: {ex.Message}");
            }
        }

        private void HandleGameResult(string resultCommand)
        {
            gameInProgress = false;
            // Parse result: "result 1-0 {White mates}"
        }

        private bool IsValidMoveFormat(string move)
        {
            // Jednostavna provera formata poteza (e2e4, a7a8q, itd.)
            return move.Length >= 4 && move.Length <= 5 &&
                   char.IsLetter(move[0]) && char.IsDigit(move[1]) &&
                   char.IsLetter(move[2]) && char.IsDigit(move[3]);
        }
    }
}
