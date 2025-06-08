using SimpletonChessEngine;
public class SearchAlgorithm
{
    private readonly Evaluator evaluator;
    private int nodesSearched = 0;

    public SearchAlgorithm(Evaluator evaluator)
    {
        this.evaluator = evaluator;
    }


    public Move FindBestMove(GameState position, int depthLimit, Func<bool> shouldStop = null)
    {
        int actualDepth = Math.Min(depthLimit, 2);

        Console.WriteLine($"info string Starting search with depth {actualDepth}");
        nodesSearched = 0;

        var board = position.GetBoard();

        // Pronađi kralja
        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 0; file < 8; file++)
            {
                int piece = board.GetPiece(file, rank);
                if (Math.Abs(piece) == 6) // King
                {
                    bool isWhiteKing = piece > 0;
                }
            }
        }

       
        var legalMoves = board.GenerateLegalMoves();

        if (legalMoves.Count == 0)
        {
            Console.WriteLine("info string No legal moves available");
            return null;
        }

        if (shouldStop?.Invoke() == true)
        {
            Console.WriteLine("info string SEARCH ACTUALLY STOPPED!");
            return null;
        }

        // Ostatak algoritma...
        var random = new Random();
        var bestMoves = new List<ChessMove>();
        int bestScore = board.IsWhiteToMove() ? int.MinValue : int.MaxValue;

        int movesToTest = Math.Min(legalMoves.Count, 15);
        Console.WriteLine($"info string Testing {movesToTest} moves");

        for (int i = 0; i < movesToTest; i++)
        {
            if (shouldStop?.Invoke() == true)
            {
                break;
            }

            var move = legalMoves[i];

            var testBoard = new Board(board);
            testBoard.MakeMove(move);

            int score = Minimax(testBoard, actualDepth - 1, int.MinValue, int.MaxValue, !board.IsWhiteToMove(), shouldStop);

            if (board.IsWhiteToMove())
            {
                if (score > bestScore)
                {
                    bestScore = score;
                    bestMoves.Clear();
                    bestMoves.Add(move);
                }
                else if (score == bestScore)
                {
                    bestMoves.Add(move);
                }
            }
            else
            {
                if (score < bestScore)
                {
                    bestScore = score;
                    bestMoves.Clear();
                    bestMoves.Add(move);
                }
                else if (score == bestScore)
                {
                    bestMoves.Add(move);
                }
            }
        }

        ChessMove bestMove = bestMoves[random.Next(bestMoves.Count)];

        if (bestMove != null)
        {
            var result = new Move
            {
                From = $"{(char)('a' + bestMove.FromFile)}{bestMove.FromRank + 1}",
                To = $"{(char)('a' + bestMove.ToFile)}{bestMove.ToRank + 1}",
                Promotion = bestMove.PromotionPiece != 0 ? "q" : ""
            };

            return result;
        }

        return null;
    }


    public int Minimax(Board board, int depth, int alpha, int beta, bool isMaximizingPlayer, Func<bool> shouldStop = null)
    {
        nodesSearched++;

        if (shouldStop?.Invoke() == true)
        {
            return 0; // ili evaluator.EvaluateBoard(board) za bolji rezultat
        }

        if (depth == 0 || board.IsCheckmate() || board.IsStalemate())
        {
            return evaluator.EvaluateBoard(board);
        }

        var moves = board.GenerateLegalMoves();

        if (isMaximizingPlayer)
        {
            int maxEval = int.MinValue;

            foreach (var move in moves)
            {
                if (shouldStop?.Invoke() == true)
                    break;


                var testBoard = new Board(board);
                testBoard.MakeMove(move);

                int eval = Minimax(testBoard, depth - 1, alpha, beta, false, shouldStop);
                maxEval = Math.Max(maxEval, eval);
                alpha = Math.Max(alpha, eval);

                if (beta <= alpha)
                    break; // Alpha-beta pruning
            }

            return maxEval;
        }
        else
        {
            int minEval = int.MaxValue;

            foreach (var move in moves)
            {
                if (shouldStop?.Invoke() == true)
                {
                    break;
                }

                var testBoard = new Board(board);
                testBoard.MakeMove(move);

                int eval = Minimax(testBoard, depth - 1, alpha, beta, true, shouldStop);
                minEval = Math.Min(minEval, eval);
                beta = Math.Min(beta, eval);

                if (beta <= alpha)
                    break; // Alpha-beta pruning
            }

            return minEval;
        }
    }
}





