using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpletonChessEngine
{
    public class GameState
    {
        private List<string> moveHistory = new List<string>();
        private Board board = new Board();

        public void Reset()
        {
            moveHistory.Clear();
            board = new Board();
            Console.Error.WriteLine("[DEBUG] GameState reset");
        }

        public void SetPosition(string fen)
        {
            Console.Error.WriteLine($"[DEBUG] SetPosition called with: {fen}");
            Reset(); // Za sada, samo reset
        }

        public void MakeMove(Move move)
        {
            if (move != null)
            {
                Console.Error.WriteLine($"[DEBUG] GameState.MakeMove called with: {move}");
                Console.Error.WriteLine($"[DEBUG] BEFORE move - whiteToMove: {board.IsWhiteToMove()}");

                moveHistory.Add(move.ToString());

                var chessMove = ChessMove.FromAlgebraic(move.ToString());
                if (chessMove != null)
                {
                    Console.Error.WriteLine($"[DEBUG] Applying move: {chessMove.ToAlgebraic()}");

                    board.MakeMove(chessMove);

                    Console.Error.WriteLine($"[DEBUG] AFTER move - whiteToMove: {board.IsWhiteToMove()}");
                }
            }
        }

        public bool IsCheckmate()
        {
            return board.IsCheckmate();
        }

        public bool IsStalemate()
        {
            return board.IsStalemate();
        }

        public bool IsDraw()
        {
            return board.IsStalemate() || IsInsufficientMaterial();
        }

        private bool IsInsufficientMaterial()
        {
            // Simple insufficient material detection
            int whitePieces = 0, blackPieces = 0;
            bool hasWhitePawn = false, hasBlackPawn = false;
            bool hasWhiteQueen = false, hasBlackQueen = false;
            bool hasWhiteRook = false, hasBlackRook = false;

            for (int rank = 0; rank < 8; rank++)
            {
                for (int file = 0; file < 8; file++)
                {
                    int piece = board.GetPiece(file, rank);
                    if (piece == Board.EMPTY) continue;

                    if (piece > 0)
                    {
                        whitePieces++;
                        if (piece == Board.WHITE_PAWN) hasWhitePawn = true;
                        if (piece == Board.WHITE_QUEEN) hasWhiteQueen = true;
                        if (piece == Board.WHITE_ROOK) hasWhiteRook = true;
                    }
                    else
                    {
                        blackPieces++;
                        if (piece == Board.BLACK_PAWN) hasBlackPawn = true;
                        if (piece == Board.BLACK_QUEEN) hasBlackQueen = true;
                        if (piece == Board.BLACK_ROOK) hasBlackRook = true;
                    }
                }
            }

            // King vs King
            if (whitePieces == 1 && blackPieces == 1) return true;

            // King + Knight/Bishop vs King
            if ((whitePieces == 2 && blackPieces == 1 && !hasWhitePawn && !hasWhiteQueen && !hasWhiteRook) ||
                (blackPieces == 2 && whitePieces == 1 && !hasBlackPawn && !hasBlackQueen && !hasBlackRook))
            {
                return true;
            }

            return false;
        }

        public int GetMoveCount()
        {
            Console.Error.WriteLine($"[DEBUG] GetMoveCount called, returning: {moveHistory.Count}");
            return moveHistory.Count;
        }

        public List<string> GetMoveHistory()
        {
            return new List<string>(moveHistory);
        }

        public Board GetBoard()
        {
            return board;
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
