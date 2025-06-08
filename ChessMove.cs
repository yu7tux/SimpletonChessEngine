using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpletonChessEngine
{
    // Chess move representation
    public class ChessMove
    {
        public int FromFile { get; set; }
        public int FromRank { get; set; }
        public int ToFile { get; set; }
        public int ToRank { get; set; }
        public int PromotionPiece { get; set; }

        public ChessMove(int fromFile, int fromRank, int toFile, int toRank, int promotionPiece = Board.EMPTY)
        {
            FromFile = fromFile;
            FromRank = fromRank;
            ToFile = toFile;
            ToRank = toRank;
            PromotionPiece = promotionPiece;
        }

        public string ToAlgebraic()
        {
            char fromFileChar = (char)('a' + FromFile);
            char toFileChar = (char)('a' + ToFile);
            string result = $"{fromFileChar}{FromRank + 1}{toFileChar}{ToRank + 1}";

            if (PromotionPiece != Board.EMPTY)
            {
                char promotionChar = PromotionPiece == Board.WHITE_QUEEN || PromotionPiece == Board.BLACK_QUEEN ? 'q' :
                                   PromotionPiece == Board.WHITE_ROOK || PromotionPiece == Board.BLACK_ROOK ? 'r' :
                                   PromotionPiece == Board.WHITE_BISHOP || PromotionPiece == Board.BLACK_BISHOP ? 'b' : 'n';
                result += promotionChar;
            }

            return result;
        }

        public static ChessMove FromAlgebraic(string moveStr)
        {
            if (moveStr.Length < 4) return null;

            int fromFile = moveStr[0] - 'a';
            int fromRank = moveStr[1] - '1';
            int toFile = moveStr[2] - 'a';
            int toRank = moveStr[3] - '1';

            int promotion = Board.EMPTY;
            if (moveStr.Length == 5)
            {
                char promChar = moveStr[4];
                bool isWhite = fromRank == 6; // Assuming white pawn promoting from 7th rank
                promotion = promChar switch
                {
                    'q' => isWhite ? Board.WHITE_QUEEN : Board.BLACK_QUEEN,
                    'r' => isWhite ? Board.WHITE_ROOK : Board.BLACK_ROOK,
                    'b' => isWhite ? Board.WHITE_BISHOP : Board.BLACK_BISHOP,
                    'n' => isWhite ? Board.WHITE_KNIGHT : Board.BLACK_KNIGHT,
                    _ => Board.EMPTY
                };
            }

            return new ChessMove(fromFile, fromRank, toFile, toRank, promotion);
        }
    }
}
