using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpletonChessEngine
{
    public class Move
    {
        public string From { get; set; } = "";
        public string To { get; set; } = "";
        public string Promotion { get; set; } = "";

        public static Move Parse(string moveStr)
        {
            Console.Error.WriteLine($"[DEBUG] Move.Parse called with: '{moveStr}'");

            if (string.IsNullOrEmpty(moveStr) || moveStr.Length < 4)
            {
                Console.Error.WriteLine("[DEBUG] Invalid move string");
                return new Move { From = "e2", To = "e4" };
            }

            var move = new Move
            {
                From = moveStr.Substring(0, 2),
                To = moveStr.Substring(2, 2),
                Promotion = moveStr.Length > 4 ? moveStr.Substring(4) : ""
            };

            Console.Error.WriteLine($"[DEBUG] Parsed move: {move}");
            return move;
        }

        public override string ToString()
        {
            string result = $"{From}{To}{Promotion}";
            Console.Error.WriteLine($"[DEBUG] Move.ToString() returning: '{result}'");
            return result;
        }

        public override bool Equals(object obj)
        {
            if (obj is Move other)
            {
                return this.From == other.From && this.To == other.To && this.Promotion == other.Promotion;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(From, To, Promotion);
        }
    }
}
