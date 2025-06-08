using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpletonChessEngine
{
    // UCI Protocol Handler
    public class UCIHandler
    {
        private readonly SimpletonChessEngine engine;
        private bool debugMode = false;

        public UCIHandler(SimpletonChessEngine engine)
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
                    if (debugMode)
                    {
                        Console.WriteLine($"info string Error: {ex.Message}");
                    }
                }
            }
        }

        public void HandleCommand(string command)
        {
            string[] parts = command.Split(' ');
            string cmd = parts[0].ToLower();

            switch (cmd)
            {
                case "uci":
                    HandleUCI();
                    break;

                case "debug":
                    if (parts.Length > 1)
                    {
                        debugMode = parts[1].ToLower() == "on";
                    }
                    break;

                case "isready":
                    Console.WriteLine("readyok");
                    break;

                case "ucinewgame":
                    engine.NewGame();
                    break;

                case "position":
                    HandlePosition(parts);
                    break;

                case "go":
                    HandleGo(parts);
                    break;

                case "stop":
                    break;

                case "quit":
                    Environment.Exit(0);
                    break;
            }
        }

        private void HandleUCI()
        {
            Console.WriteLine("id name Simpleton Chess Engine 1.0");
            Console.WriteLine("id author VašeIme");
            Console.WriteLine("option name Hash type spin default 64 min 1 max 1024");
            Console.WriteLine("uciok");
        }

        private void HandlePosition(string[] parts)
        {
            if (parts.Length < 2) return;

            Console.Error.WriteLine($"[DEBUG] HandlePosition: {string.Join(" ", parts)}");

            if (parts[1] == "startpos")
            {
                // NE resetuj uvek - samo ako nema moves
                int movesIndex = Array.IndexOf(parts, "moves");

                if (movesIndex <= 0)
                {
                    // Nema poteza, resetuj
                    engine.NewGame();
                    Console.Error.WriteLine("[DEBUG] No moves, resetting game");
                }
                else
                {
                    // Ima poteze, resetuj i primeni sve
                    engine.NewGame();
                    Console.Error.WriteLine($"[DEBUG] Found moves starting at index {movesIndex}");

                    for (int i = movesIndex + 1; i < parts.Length; i++)
                    {
                        Console.Error.WriteLine($"[DEBUG] Applying move: {parts[i]}");
                        engine.MakeMove(parts[i]);
                    }
                }
            }
        }

        private void HandleGo(string[] parts)
        {
            try
            {
                Console.Error.WriteLine("[DEBUG] Starting search...");
                string bestMove = engine.GetBestMove();
                Console.Error.WriteLine($"[DEBUG] Found move: {bestMove}");

                if (string.IsNullOrEmpty(bestMove) || bestMove == "null")
                {
                    bestMove = "e2e4"; // Hard fallback
                }

                Console.WriteLine($"bestmove {bestMove}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[DEBUG] Search error: {ex.Message}");
                Console.WriteLine("bestmove e2e4"); // Safe fallback
            }
        }
    }
}
