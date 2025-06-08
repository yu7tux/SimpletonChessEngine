using SimpletonChessEngine;
public class SearchAlgorithm
{
    private readonly Evaluator evaluator;
    private int nodesSearched = 0; // Dodano!

    public SearchAlgorithm(Evaluator evaluator)
    {
        this.evaluator = evaluator;
    }

    // Dodaj debug u SearchAlgorithm.FindBestMove na vrh:
    public Move FindBestMove(GameState position, int depthLimit)
    {
        int actualDepth = Math.Min(depthLimit, 2);

        Console.WriteLine($"info string Starting search with depth {actualDepth}");
        nodesSearched = 0;

        var board = position.GetBoard();

        //Console.WriteLine($"info string === SEARCH START ===");
        //Console.WriteLine($"info string Original board whiteToMove: {board.IsWhiteToMove()}");

        //Console.WriteLine("info string === TURN VERIFICATION ===");
        //Console.WriteLine($"info string Engine thinks it's {(board.IsWhiteToMove() ? "WHITE" : "BLACK")} turn");
        //Console.WriteLine("info string GUI position shows 6 moves played:");
        //Console.WriteLine("info string 1.e4 e5 2.Nf3 Nc6 3.d4 Nxd4 4.Nxd4 g6 5.Bc4 f6 6.Qf3");
        //Console.WriteLine("info string After 6 moves, it should be BLACK to move!");
        //Console.WriteLine("info string === END TURN VERIFICATION ===");

        Console.WriteLine("info string === CASTLING DEBUG ===");
        Console.WriteLine($"info string White to move: {board.IsWhiteToMove()}");
        Console.WriteLine($"info string White castling rights: K={board.WhiteCanCastleKingside}, Q={board.WhiteCanCastleQueenside}");
        Console.WriteLine($"info string Black castling rights: K={board.BlackCanCastleKingside}, Q={board.BlackCanCastleQueenside}");

        // Pronađi kralja
        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 0; file < 8; file++)
            {
                int piece = board.GetPiece(file, rank);
                if (Math.Abs(piece) == 6) // King
                {
                    bool isWhiteKing = piece > 0;
                    Console.WriteLine($"info string Found {(isWhiteKing ? "White" : "Black")} king at {(char)('a' + file)}{rank + 1}");

                    if (isWhiteKing && board.IsWhiteToMove())
                    {
                        Console.WriteLine("info string White king found and white to move - checking castling squares:");
                        Console.WriteLine($"info string f1 piece: {board.GetPiece(5, 0)} (should be 0 for empty)");
                        Console.WriteLine($"info string g1 piece: {board.GetPiece(6, 0)} (should be 0 for empty)");
                        Console.WriteLine($"info string h1 piece: {board.GetPiece(7, 0)} (should be 2 for rook)");
                    }
                    else if (!isWhiteKing && !board.IsWhiteToMove())
                    {
                        Console.WriteLine("info string Black king found and black to move - checking castling squares:");
                        Console.WriteLine($"info string f8 piece: {board.GetPiece(5, 7)} (should be 0 for empty)");
                        Console.WriteLine($"info string g8 piece: {board.GetPiece(6, 7)} (should be 0 for empty)");
                        Console.WriteLine($"info string h8 piece: {board.GetPiece(7, 7)} (should be -2 for rook)");
                    }
                }
            }
        }

        //// DODAJ OVAJ DEBUG
        //Console.WriteLine("info string === ENGINE BOARD STATE ===");
        //for (int rank = 7; rank >= 0; rank--)
        //{
        //    string rankStr = $"{rank + 1} ";
        //    for (int file = 0; file < 8; file++)
        //    {
        //        int piece = board.GetPiece(file, rank);
        //        char symbol = piece switch
        //        {
        //            6 => 'K',
        //            5 => 'Q',
        //            2 => 'R',
        //            4 => 'B',
        //            3 => 'N',
        //            1 => 'P',
        //            -6 => 'k',
        //            -5 => 'q',
        //            -2 => 'r',
        //            -4 => 'b',
        //            -3 => 'n',
        //            -1 => 'p',
        //            _ => '.'
        //        };
        //        rankStr += symbol + " ";
        //    }
        //    Console.WriteLine($"info string {rankStr}");
        //}
        //Console.WriteLine($"info string   a b c d e f g h");
        //Console.WriteLine($"info string Turn: {(board.IsWhiteToMove() ? "White" : "Black")}");


        //// Debug castling rights
        //Console.WriteLine($"info string White castling: K={board.WhiteCanCastleKingside}, Q={board.WhiteCanCastleQueenside}");
        //Console.WriteLine($"info string Black castling: K={board.BlackCanCastleQueenside}, Q={board.BlackCanCastleQueenside}");

        // Debug board position (samo važne figure)
        //Console.WriteLine("info string Board state:");
        //for (int rank = 7; rank >= 0; rank--)  // 8-1
        //{
        //    string line = $"info string {rank + 1}: ";
        //    for (int file = 0; file < 8; file++)  // a-h
        //    {
        //        int piece = board.Squares[file, rank];
        //        char symbol = piece switch
        //        {
        //            1 => 'P',
        //            -1 => 'p',  // Pawn
        //            2 => 'R',
        //            -2 => 'r',  // Rook  
        //            3 => 'N',
        //            -3 => 'n',  // Knight
        //            4 => 'B',
        //            -4 => 'b',  // Bishop
        //            5 => 'Q',
        //            -5 => 'q',  // Queen
        //            6 => 'K',
        //            -6 => 'k',  // King
        //            _ => '.'
        //        };
        //        line += symbol;
        //    }
        //    Console.WriteLine(line);
        //}
        //Console.WriteLine("info string    abcdefgh");


        // Specifično proveri f1 i c4
        //int f1Piece = board.GetPiece(5, 0); // f1
        //int c4Piece = board.GetPiece(2, 3); // c4
        //Console.WriteLine($"info string f1 contains: {f1Piece}");
        //Console.WriteLine($"info string c4 contains: {c4Piece}");

        // GUI pozicija je: 1.e4 e5 2.Nf3 Nc6 3.d4 Nxd4 4.Nxd4 a5 5.Nf5
        //Console.WriteLine("info string Expected GUI position after: 1.e4 e5 2.Nf3 Nc6 3.d4 Nxd4 4.Nxd4 a5 5.Nf5");
        //Console.WriteLine("info string White should have: King e1, Queen d1, Rooks a1/h1, Bishops c1/f1, Knights b1/f5, Pawns a2/b2/c2/f2/g2/h2/e4");
        //Console.WriteLine("info string Black should have: King e8, Queen d8, Rooks a8/h8, Bishops c8/f8, Knights b8/g8, Pawns b7/c7/d7/f7/g7/h7/e5/a5");

        //Console.WriteLine("info string === END ENGINE BOARD ===");

        var legalMoves = board.GenerateLegalMoves();

        //Console.WriteLine($"info string After GenerateLegalMoves - whiteToMove: {board.IsWhiteToMove()}");
        //Console.WriteLine($"info string Found {legalMoves.Count} legal moves");
        //Console.WriteLine($"info string Available moves: {string.Join(", ", legalMoves.Take(10).Select(m => m.ToAlgebraic()))}");


        // Pretraži rokadu
        bool hasCastling = false;


        bool foundCastling = false;
        foreach (var move in legalMoves)
        {
            string moveStr = move.ToAlgebraic();
            if (moveStr.Contains("O-O") ||
                (move.FromFile == 4 && move.ToFile == 6) || // e1-g1 or e8-g8
                (move.FromFile == 4 && move.ToFile == 2))   // e1-c1 or e8-c8
            {
                Console.WriteLine($"info string Found castling move: {moveStr}");
                foundCastling = true;
            }
        }

        if (!foundCastling)
        {
            Console.WriteLine("info string NO CASTLING MOVES FOUND!");
        }

        //// Proveri da li f1c4 postoji
        //bool f1c4Exists = legalMoves.Any(m => m.FromFile == 5 && m.FromRank == 0 && m.ToFile == 2 && m.ToRank == 3);
        //Console.WriteLine($"info string f1c4 in legal moves: {f1c4Exists}");

        if (legalMoves.Count == 0)
        {
            Console.WriteLine("info string No legal moves available");
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
            var move = legalMoves[i];

            var testBoard = new Board(board);
            testBoard.MakeMove(move);

            int score = Minimax(testBoard, actualDepth - 1, int.MinValue, int.MaxValue, !board.IsWhiteToMove());

            Console.WriteLine($"info string Move {move.ToAlgebraic()}: score {score}");

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

        Console.WriteLine($"info string Best score: {bestScore}, Found {bestMoves.Count} equally good moves");
        Console.WriteLine($"info string Selected move: {bestMove.ToAlgebraic()}");

        if (bestMove != null)
        {
            var result = new Move
            {
                From = $"{(char)('a' + bestMove.FromFile)}{bestMove.FromRank + 1}",
                To = $"{(char)('a' + bestMove.ToFile)}{bestMove.ToRank + 1}",
                Promotion = bestMove.PromotionPiece != 0 ? "q" : ""
            };

            Console.WriteLine($"info string Final move string: '{result.ToString()}'");

            Console.WriteLine($"info string === SEARCH END ===");
            Console.WriteLine($"info string Final original board whiteToMove: {board.IsWhiteToMove()}");


            return result;
        }

        return null;
    }

    //public Move FindBestMove(GameState position, int depthLimit)
    //{
    //    int actualDepth = Math.Min(depthLimit, 2);

    //    Console.WriteLine($"info string Starting search with depth {actualDepth}");
    //    nodesSearched = 0;

    //    var board = position.GetBoard();
    //    var legalMoves = board.GenerateLegalMoves();

    //    Console.WriteLine($"info string Found {legalMoves.Count} legal moves");
    //    Console.WriteLine($"info string Available moves: {string.Join(", ", legalMoves.Take(10).Select(m => m.ToAlgebraic()))}");

    //    // debug


    //    // debug

    //    if (legalMoves.Count == 0)
    //    {
    //        Console.WriteLine("info string No legal moves available");
    //        return null;
    //    }

    //    // Dodaj random element da izbegneš zacikljavanje
    //    var random = new Random();
    //    var bestMoves = new List<ChessMove>(); // Lista najboljih poteza
    //    int bestScore = board.IsWhiteToMove() ? int.MinValue : int.MaxValue;

    //    int movesToTest = Math.Min(legalMoves.Count, 15); // Povećaj broj testiranih poteza
    //    Console.WriteLine($"info string Testing {movesToTest} moves");

    //    for (int i = 0; i < movesToTest; i++)
    //    {
    //        var move = legalMoves[i];

    //        var testBoard = new Board(board);
    //        testBoard.MakeMove(move);

    //        int score = Minimax(testBoard, actualDepth - 1, int.MinValue, int.MaxValue, !board.IsWhiteToMove());

    //        Console.WriteLine($"info string Move {move.ToAlgebraic()}: score {score}");

    //        if (board.IsWhiteToMove())
    //        {
    //            if (score > bestScore)
    //            {
    //                bestScore = score;
    //                bestMoves.Clear();
    //                bestMoves.Add(move);
    //            }
    //            else if (score == bestScore)
    //            {
    //                bestMoves.Add(move); // Dodaj u listu jednako dobrih poteza
    //            }
    //        }
    //        else
    //        {
    //            if (score < bestScore)
    //            {
    //                bestScore = score;
    //                bestMoves.Clear();
    //                bestMoves.Add(move);
    //            }
    //            else if (score == bestScore)
    //            {
    //                bestMoves.Add(move);
    //            }
    //        }
    //    }

    //    // Izaberi random između najboljih poteza
    //    ChessMove bestMove = bestMoves[random.Next(bestMoves.Count)];

    //    Console.WriteLine($"info string Best score: {bestScore}, Found {bestMoves.Count} equally good moves");
    //    Console.WriteLine($"info string Selected move: {bestMove.ToAlgebraic()}");

    //    if (bestMove != null)
    //    {
    //        var result = new Move
    //        {
    //            From = $"{(char)('a' + bestMove.FromFile)}{bestMove.FromRank + 1}",
    //            To = $"{(char)('a' + bestMove.ToFile)}{bestMove.ToRank + 1}",
    //            Promotion = bestMove.PromotionPiece != 0 ? "q" : ""
    //        };

    //        Console.WriteLine($"info string Final move string: '{result.ToString()}'");
    //        return result;
    //    }

    //    return null;
    //}


    // Dodano: Minimax algoritam!
    public int Minimax(Board board, int depth, int alpha, int beta, bool isMaximizingPlayer)
    {
        nodesSearched++;

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
                var testBoard = new Board(board);
                testBoard.MakeMove(move);

                int eval = Minimax(testBoard, depth - 1, alpha, beta, false);
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
                var testBoard = new Board(board);
                testBoard.MakeMove(move);

                int eval = Minimax(testBoard, depth - 1, alpha, beta, true);
                minEval = Math.Min(minEval, eval);
                beta = Math.Min(beta, eval);

                if (beta <= alpha)
                    break; // Alpha-beta pruning
            }

            return minEval;
        }
    }
}





