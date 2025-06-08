using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpletonChessEngine
{
    // Zameni celu GameState klasu sa ovom verzijom
    public class GameState
    {
        private List<string> moveHistory = new List<string>();

        public void Reset()
        {
            moveHistory.Clear();
            Console.Error.WriteLine("[DEBUG] GameState reset");
        }

        public void SetPosition(string fen)
        {
            Console.Error.WriteLine($"[DEBUG] SetPosition called with: {fen}");
            // Za sada, jednostavno reset - kasnije dodaj FEN parsing
            Reset();
        }

        public void MakeMove(Move move)
        {
            if (move != null)
            {
                moveHistory.Add(move.ToString());
                Console.Error.WriteLine($"[DEBUG] Move added: {move}, total moves: {moveHistory.Count}");
            }
        }

        public bool IsLegalMove(Move move)
        {
            // Za sada, svi potezi su "legalni"
            return move != null && !string.IsNullOrEmpty(move.From) && !string.IsNullOrEmpty(move.To);
        }

        public bool IsCheckmate() { return false; }
        public bool IsStalemate() { return false; }
        public bool IsDraw() { return false; }

        // Nova metoda koja nedostaje
        public int GetMoveCount()
        {
            Console.Error.WriteLine($"[DEBUG] GetMoveCount called, returning: {moveHistory.Count}");
            return moveHistory.Count;
        }

        public List<string> GetMoveHistory()
        {
            return new List<string>(moveHistory);
        }

        // Helper metoda za debugging
        public void PrintMoveHistory()
        {
            Console.Error.WriteLine($"[DEBUG] Move history ({moveHistory.Count} moves):");
            for (int i = 0; i < moveHistory.Count; i++)
            {
                Console.Error.WriteLine($"[DEBUG] {i + 1}. {moveHistory[i]}");
            }
        }
    }
}
