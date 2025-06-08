using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpletonChessEngine
{
    public class Evaluator
    {
        public int Evaluate(GameState position)
        {
            return EvaluateBoard(position.GetBoard());
        }

        public int EvaluateBoard(Board board)
        {
            if (board.IsCheckmate())
            {
                return board.IsWhiteToMove() ? -999999 : 999999; // Mate score
            }

            if (board.IsStalemate())
            {
                return 0; // Draw
            }

            int score = 0;

            // Material evaluation
            score += board.Evaluate();

            // Positional bonuses
            score += EvaluatePosition(board);

            return score;
        }

        private int EvaluatePosition(Board board)
        {
            int positionalScore = 0;

            // Center control bonus
            for (int file = 3; file <= 4; file++)
            {
                for (int rank = 3; rank <= 4; rank++)
                {
                    int piece = board.GetPiece(file, rank);
                    if (piece != Board.EMPTY)
                    {
                        positionalScore += piece > 0 ? 10 : -10;
                    }
                }
            }

            // King safety (simplified)
            if (!IsEndgame(board))
            {
                positionalScore += EvaluateKingSafety(board, true) - EvaluateKingSafety(board, false);
            }

            return positionalScore;
        }

        private bool IsEndgame(Board board)
        {
            int pieceCount = 0;
            for (int rank = 0; rank < 8; rank++)
            {
                for (int file = 0; file < 8; file++)
                {
                    if (board.GetPiece(file, rank) != Board.EMPTY)
                        pieceCount++;
                }
            }
            return pieceCount <= 12; // Simple endgame detection
        }

        private int EvaluateKingSafety(Board board, bool isWhite)
        {
            int kingType = isWhite ? Board.WHITE_KING : Board.BLACK_KING;
            int safety = 0;

            // Find king
            for (int rank = 0; rank < 8; rank++)
            {
                for (int file = 0; file < 8; file++)
                {
                    if (board.GetPiece(file, rank) == kingType)
                    {
                        // Penalty for exposed king
                        if (isWhite && rank > 2) safety -= 50;
                        if (!isWhite && rank < 5) safety -= 50;

                        return safety;
                    }
                }
            }

            return safety;
        }
    }
}
