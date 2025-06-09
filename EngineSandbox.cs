using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpletonChessEngine
{
    public class EngineSandbox
    {
        private readonly GameState sandboxState;
        private readonly string originalFen;

        public EngineSandbox(GameState baseState)
        {
            originalFen = baseState.GetFen();
            sandboxState = new GameState();
            sandboxState.SetPosition(originalFen);
        }

        public GameState GetState()
        {
            return sandboxState;
        }

        public void Reset()
        {
            sandboxState.SetPosition(originalFen);
        }

        public bool TryMakeMove(Move move)
        {
            try
            {
                sandboxState.MakeMove(move);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool IsCheckmate()
        {
            var board = sandboxState.GetBoard();
            var engine = new PathDependentEngine(); // Kreiraj instancu
            var legal = engine.GenerateLegalMoves(sandboxState);
            return legal.Count == 0 && engine.IsKingInCheck(board.IsWhiteToMove()); // Koristi istu instancu
        }
    }

}
