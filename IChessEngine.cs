using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpletonChessEngine
{
    public interface IChessEngine
    {
        //string GetBestMove(string position = null);
        //bool ShouldStop { get; set; }
        //void SetPosition(string fen);

        string GetBestMove(string position = null);
        bool ShouldStop { get; set; }
        void SetPosition(string fen);
        void SetPosition(string[] moves);

        // Dodaj ove:
        Task RunLichessBot();
        void RunWinBoard();
        void RunUCI();
        void RunAutoDetect();

        void NewGame();
        void MakeMove(string move);
    }
}
