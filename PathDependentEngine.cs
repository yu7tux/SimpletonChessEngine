using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpletonChessEngine
{
    /// <summary>
    /// Chess engine baziran na konceptima iz radova o asymptotic probability,
    /// path-dependent emergence i constraint propagation
    /// </summary>
    public class PathDependentEngine : IChessEngine
    {
        public volatile bool shouldStop = false;
        public bool ShouldStop
        {
            get => shouldStop;
            set => shouldStop = value;
        }

        private readonly GameState gameState;
        private Board board;

        // Constraint network komponente
        private readonly ConstraintNetwork constraintNetwork;
        private readonly PathMemory pathMemory;
        private readonly TensorProbabilitySpace probabilitySpace;

        // Parametri za path-dependent evaluation
        private const double CONVERGENCE_THRESHOLD = 0.01;
        private const int MAX_PATH_LENGTH = 10;
        private const double CONSTRAINT_PROPAGATION_RATE = 0.8;
        private const int MINIMAX_DEPTH = 3;

        // Random generator za variabilnost
        private readonly Random random = new Random();

        // Cache za pozicije (da izbegnemo re-evaluaciju)
        private Dictionary<string, double> positionCache = new Dictionary<string, double>();

        public PathDependentEngine()
        {
            gameState = new GameState();
            board = gameState.GetBoard();
            constraintNetwork = new ConstraintNetwork();
            pathMemory = new PathMemory();
            probabilitySpace = new TensorProbabilitySpace();
        }

        public string GetBestMove(string position = null)
        {
            Console.WriteLine("info string PathDependentEngine analyzing position...");

            if (!string.IsNullOrEmpty(position))
            {
                gameState.SetPosition(position);
            }

            board = gameState.GetBoard();
            var legalMoves = GenerateLegalMoves();
            if (legalMoves.Count == 0)
            {
                return "e2e4"; // fallback
            }

            // Jednostavnija evaluacija sa path-dependent twist
            var moveEvaluations = new Dictionary<Move, double>();

            foreach (var move in legalMoves)
            {
                if (shouldStop) break;

                // Osnovna evaluacija poteza
                double baseEval = GetBaseMoveEvaluation(move);

                // Path-dependent modifikacija
                var paths = GeneratePathPermutations(move, 2);
                double pathModifier = 0.0;

                foreach (var path in paths)
                {
                    double pathValue = 0.0;
                    for (int i = 0; i < path.Count; i++)
                    {
                        double sign = (i % 2 == 0) ? 1.0 : -1.0;
                        pathValue += sign * path[i] * 0.01;
                    }
                    pathModifier += pathValue;
                }

                double finalEval = baseEval + pathModifier / paths.Count;
                moveEvaluations[move] = finalEval;
            }

            // Odaberi najbolji potez
            if (moveEvaluations.Count == 0)
            {
                return legalMoves[0].ToString();
            }

            var sortedMoves = moveEvaluations.OrderByDescending(kvp => kvp.Value).ToList();

            // Prikaži top 5 poteza
            Console.WriteLine("info string Top moves:");
            foreach (var kvp in sortedMoves.Take(5))
            {
                Console.WriteLine($"info string   {kvp.Key}: {kvp.Value:F3}");
            }

            var bestMove = sortedMoves.First().Key;
            Console.WriteLine($"info string Best move: {bestMove}");

            return bestMove.ToString();
        }

        /// <summary>
        /// Path-dependent MiniMax - evaluacija zavisi od path-a kroz stablo
        /// </summary>
        private double PathDependentMiniMax(Move move, int depth, bool isMaximizing,
            double alpha, double beta, List<int> path, List<Constraint> constraints)
        {
            // Ako je depth 0, samo evaluiraj trenutnu poziciju
            if (depth == 0)
            {
                return PathDependentEvaluation(path, constraints);
            }

            // Za sada, pojednostavljena evaluacija
            // Koristi brzu evaluaciju poteza + path modifikator
            double baseEval = GetBaseMoveEvaluation(move);

            // Path-dependent modifikacija
            double pathModifier = 0.0;
            for (int i = 0; i < path.Count; i++)
            {
                double sign = (i % 2 == 0) ? 1.0 : -1.0;
                pathModifier += sign * path[i] * 0.1;
            }

            // Constraint modifikacija
            foreach (var constraint in constraints)
            {
                baseEval *= (1.0 + constraint.Strength * 0.05);
            }

            return baseEval + pathModifier;
        }

        /// <summary>
        /// Path-dependent evaluation - vrednost pozicije zavisi od path-a
        /// </summary>
        private double PathDependentEvaluation(List<int> path, List<Constraint> constraints)
        {
            double eval = 0.0;

            // Osnovni materijal
            for (int rank = 0; rank < 8; rank++)
            {
                for (int file = 0; file < 8; file++)
                {
                    int piece = board.GetPiece(file, rank);
                    if (piece != Board.EMPTY)
                    {
                        double value = GetPieceValue(Math.Abs(piece));
                        eval += (piece > 0) ? value : -value;
                    }
                }
            }

            // Path-dependent modifikacija bazirano na constraints
            foreach (var constraint in constraints)
            {
                eval *= (1.0 + constraint.Strength * 0.01);
            }

            // Alternating series baziran na path-u
            double pathModifier = 0.0;
            for (int i = 0; i < path.Count; i++)
            {
                double sign = (i % 2 == 0) ? 1.0 : -1.0;
                pathModifier += sign * path[i] * 0.01;
            }

            eval += pathModifier;

            return eval;
        }

        /// <summary>
        /// Generiše različite path permutacije za početni potez
        /// </summary>
        private List<List<int>> GeneratePathPermutations(Move initialMove, int numPaths)
        {
            var paths = new List<List<int>>();

            for (int i = 0; i < numPaths; i++)
            {
                var path = new List<int>();
                // Različite početne permutacije
                for (int j = 0; j < 3; j++)
                {
                    path.Add((i + j * 7) % 10); // Pseudo-random ali determinističko
                }
                paths.Add(path);
            }

            return paths;
        }

        /// <summary>
        /// Permutuje poteze na osnovu path-a
        /// </summary>
        private List<Move> PermuteMoves(List<Move> moves, List<int> path)
        {
            if (path.Count == 0) return moves;

            // Koristi path za determinističku permutaciju
            var seed = path.Sum() % 100;
            var rng = new Random(seed);

            return moves.OrderBy(m => rng.Next()).ToList();
        }

        /// <summary>
        /// Generiše legalne poteze za dati board state
        /// </summary>
        private List<Move> GenerateLegalMovesForBoard(Board boardState)
        {
            var savedBoard = board;
            board = boardState;
            var moves = GenerateLegalMoves();
            board = savedBoard;
            return moves;
        }

        /// <summary>
        /// Vraća FEN reprezentaciju trenutne pozicije
        /// </summary>
        private string GetCurrentFEN()
        {
            // Simplified FEN - samo za test
            return "current_position";
        }

        /// <summary>
        /// Evaluacija poteza kroz multiple paths sa conditional convergence
        /// </summary>
        private double EvaluateMoveThroughPaths(Move move)
        {
            var pathEvaluations = new List<double>();
            var constraints = constraintNetwork.GetActiveConstraints(gameState, move);

            // Dodaj osnovnu heuristiku za različite tipove poteza
            double baseEval = GetBaseMoveEvaluation(move);

            // Generiši različite paths (permutacije budućih poteza)
            var futurePaths = GenerateFuturePaths(move, Math.Min(MAX_PATH_LENGTH, 3)); // Smanji depth za brzinu

            foreach (var path in futurePaths)
            {
                // Svaki path ima svoju trajektoriju kroz phase space
                double pathValue = ComputePathValue(path, constraints);
                pathEvaluations.Add(baseEval + pathValue);
            }

            // Conditional convergence - različite permutacije daju različite sume
            return ComputeConditionalConvergence(pathEvaluations);
        }

        private double GetBaseMoveEvaluation(Move move)
        {
            double eval = 0.0;

            string from = move.From;
            string to = move.To;
            int fromFile = from[0] - 'a';
            int fromRank = from[1] - '1';
            int toFile = to[0] - 'a';
            int toRank = to[1] - '1';

            int movingPiece = board.GetPiece(fromFile, fromRank);
            int targetPiece = board.GetPiece(toFile, toRank);

            // Debug info
            Console.WriteLine($"info string Evaluating {move}: piece {movingPiece} to square with {targetPiece}");

            // CAPTURE BONUS - najvažnije!
            if (targetPiece != Board.EMPTY)
            {
                double captureValue = GetPieceValue(Math.Abs(targetPiece));
                eval += captureValue * 10.0;
                Console.WriteLine($"info string   Capture bonus: +{captureValue * 10.0}");
            }

            // Centralizacija
            double centerBonus = 0.0;
            if (toFile >= 2 && toFile <= 5 && toRank >= 2 && toRank <= 5)
            {
                centerBonus = 0.3;
                if ((toFile == 3 || toFile == 4) && (toRank == 3 || toRank == 4))
                {
                    centerBonus = 0.5;
                }
            }
            eval += centerBonus;

            // Razvitak figura (ne pešaci)
            if (Math.Abs(movingPiece) != Board.WHITE_PAWN)
            {
                bool isWhite = movingPiece > 0;
                if ((isWhite && fromRank == 0) || (!isWhite && fromRank == 7))
                {
                    eval += 0.4; // Bonus za razvitak
                }
            }

            // Pawn advancement
            if (Math.Abs(movingPiece) == Board.WHITE_PAWN)
            {
                bool isWhite = movingPiece > 0;
                if (isWhite)
                {
                    eval += (toRank - fromRank) * 0.1;
                }
                else
                {
                    eval += (fromRank - toRank) * 0.1;
                }
            }

            // Random factor - VRLO MALI
            eval += (random.NextDouble() - 0.5) * 0.02;

            Console.WriteLine($"info string   Total eval for {move}: {eval:F3}");

            return eval;
        }

        private double GetPieceValue(int pieceType)
        {
            switch (pieceType)
            {
                case Board.WHITE_PAWN: return 1.0;
                case Board.WHITE_KNIGHT: return 3.0;
                case Board.WHITE_BISHOP: return 3.0;
                case Board.WHITE_ROOK: return 5.0;
                case Board.WHITE_QUEEN: return 9.0;
                default: return 0.0;
            }
        }

        /// <summary>
        /// Generiše moguće future paths (trajektorije kroz game tree)
        /// </summary>
        private List<List<Move>> GenerateFuturePaths(Move initialMove, int depth)
        {
            var paths = new List<List<Move>>();

            // Simpler implementation without Clone
            // Zapamti trenutno stanje
            var originalBoard = board;
            board = gameState.GetBoard();

            // Privremeno primeni potez
            gameState.MakeMove(initialMove);

            // Rekurzivno generiši paths
            GeneratePathsRecursive(new List<Move> { initialMove }, paths, depth - 1);

            // Vrati na originalno stanje (ovo nije idealno, ali radi za testiranje)
            // U produkciji bi trebalo implementirati proper undo

            return paths.Take(10).ToList(); // Ograniči na 10 paths za performanse
        }

        private void GeneratePathsRecursive(List<Move> currentPath,
            List<List<Move>> allPaths, int remainingDepth)
        {
            if (remainingDepth <= 0 || shouldStop)
            {
                allPaths.Add(new List<Move>(currentPath));
                return;
            }

            var moves = GenerateLegalMoves().Take(3).ToList(); // Top 3 poteza
            foreach (var move in moves)
            {
                // Simplified - samo dodaj u path bez actual board updates
                currentPath.Add(move);
                GeneratePathsRecursive(currentPath, allPaths, remainingDepth - 1);
                currentPath.RemoveAt(currentPath.Count - 1);
            }
        }

        /// <summary>
        /// Računanje vrednosti path-a kroz constraint propagation
        /// </summary>
        private double ComputePathValue(List<Move> path, List<Constraint> initialConstraints)
        {
            double value = 0.0;
            var activeConstraints = new List<Constraint>(initialConstraints);

            for (int i = 0; i < path.Count; i++)
            {
                var move = path[i];

                // Constraint propagation kroz path
                activeConstraints = constraintNetwork.PropagateConstraints(
                    activeConstraints, move, CONSTRAINT_PROPAGATION_RATE);

                // Tensor probability calculation
                var tensorProb = probabilitySpace.ComputeTensorProbability(
                    gameState, move, activeConstraints, i);

                // Path-dependent contribution
                value += tensorProb * Math.Pow(0.9, i); // Decay factor
            }

            return value;
        }

        /// <summary>
        /// Conditional convergence calculation - ključni deo iz rada
        /// </summary>
        private double ComputeConditionalConvergence(List<double> values)
        {
            if (values.Count == 0) return 0.0;

            // Sortiraj vrednosti za različite permutacije
            var sortedAscending = values.OrderBy(v => v).ToList();
            var sortedDescending = values.OrderByDescending(v => v).ToList();

            // Računaj sume za različite orderings (conditional convergence)
            double sumAscending = 0.0;
            double sumDescending = 0.0;

            for (int i = 0; i < values.Count; i++)
            {
                // Alternating series style
                double sign = (i % 2 == 0) ? 1.0 : -1.0;
                sumAscending += sign * sortedAscending[i];
                sumDescending += sign * sortedDescending[i];
            }

            // Path-dependent rezultat zavisi od ordering-a
            double convergenceValue = (sumAscending + sumDescending) / 2.0;

            // Normalizacija na [0, 1] interval
            return Math.Tanh(convergenceValue);
        }

        /// <summary>
        /// Generiše sve legalne poteze iz trenutne pozicije
        /// </summary>
        public List<Move> GenerateLegalMoves()
        {
            var moves = new List<Move>();
            board = gameState.GetBoard(); // Osiguraj da imamo najnoviji board

            // Prvo proveri da li smo u šahu
            bool inCheck = IsKingInCheck(board.IsWhiteToMove());

            // Generiši sve moguće poteze
            for (int fromRank = 0; fromRank < 8; fromRank++)
            {
                for (int fromFile = 0; fromFile < 8; fromFile++)
                {
                    int piece = board.GetPiece(fromFile, fromRank);
                    if (piece == Board.EMPTY) continue;

                    // Proveri da li je figura na potezu
                    bool isWhitePiece = piece > 0;
                    if (isWhitePiece != board.IsWhiteToMove()) continue;

                    // Generiši poteze za ovu figuru
                    GenerateMovesForPiece(fromFile, fromRank, piece, moves);
                }
            }

            // Filtriraj ilegalne poteze (oni koji ostavljaju kralja u šahu)
            return FilterLegalMoves(moves);
        }

        // Overload koji prima GameState (za kompatibilnost sa EngineSandbox)
        public List<Move> GenerateLegalMoves(GameState state)
        {
            // Samo koristi trenutni board iz prosleđenog state-a
            var savedBoard = board;
            board = state.GetBoard();

            var moves = GenerateLegalMoves();

            board = savedBoard; // Vrati originalni board
            return moves;
        }

        public bool IsKingInCheck(bool whiteKing)
        {
            // Nađi kralja
            int kingFile = -1, kingRank = -1;
            for (int rank = 0; rank < 8; rank++)
            {
                for (int file = 0; file < 8; file++)
                {
                    int piece = board.GetPiece(file, rank);
                    if ((whiteKing && piece == Board.WHITE_KING) ||
                        (!whiteKing && piece == Board.BLACK_KING))
                    {
                        kingFile = file;
                        kingRank = rank;
                        break;
                    }
                }
                if (kingFile >= 0) break;
            }

            if (kingFile < 0) return false; // Kralj nije pronađen?

            // Proveri da li je kralj napadnut
            return IsSquareAttacked(kingFile, kingRank, !whiteKing);
        }

        private void GenerateMovesForPiece(int fromFile, int fromRank, int piece, List<Move> moves)
        {
            int absPiece = Math.Abs(piece);

            switch (absPiece)
            {
                case Board.WHITE_PAWN:
                    GeneratePawnMoves(fromFile, fromRank, piece > 0, moves);
                    break;
                case Board.WHITE_KNIGHT:
                    GenerateKnightMoves(fromFile, fromRank, piece > 0, moves);
                    break;
                case Board.WHITE_BISHOP:
                    GenerateBishopMoves(fromFile, fromRank, piece > 0, moves);
                    break;
                case Board.WHITE_ROOK:
                    GenerateRookMoves(fromFile, fromRank, piece > 0, moves);
                    break;
                case Board.WHITE_QUEEN:
                    GenerateQueenMoves(fromFile, fromRank, piece > 0, moves);
                    break;
                case Board.WHITE_KING:
                    GenerateKingMoves(fromFile, fromRank, piece > 0, moves);
                    break;
            }
        }

        private void GeneratePawnMoves(int file, int rank, bool isWhite, List<Move> moves)
        {
            int direction = isWhite ? 1 : -1;
            int startRank = isWhite ? 1 : 6;

            // Pomeranje napred
            if (rank + direction >= 0 && rank + direction < 8)
            {
                if (board.GetPiece(file, rank + direction) == Board.EMPTY)
                {
                    moves.Add(new Move
                    {
                        From = $"{(char)('a' + file)}{rank + 1}",
                        To = $"{(char)('a' + file)}{rank + direction + 1}"
                    });

                    // Duplo pomeranje sa početne pozicije
                    if (rank == startRank && board.GetPiece(file, rank + 2 * direction) == Board.EMPTY)
                    {
                        moves.Add(new Move
                        {
                            From = $"{(char)('a' + file)}{rank + 1}",
                            To = $"{(char)('a' + file)}{rank + 2 * direction + 1}"
                        });
                    }
                }

                // Captures
                for (int df = -1; df <= 1; df += 2)
                {
                    int newFile = file + df;
                    if (newFile >= 0 && newFile < 8)
                    {
                        int targetPiece = board.GetPiece(newFile, rank + direction);
                        if (targetPiece != Board.EMPTY && (targetPiece > 0) != isWhite)
                        {
                            moves.Add(new Move
                            {
                                From = $"{(char)('a' + file)}{rank + 1}",
                                To = $"{(char)('a' + newFile)}{rank + direction + 1}"
                            });
                        }
                    }
                }
            }
        }

        private void GenerateKnightMoves(int file, int rank, bool isWhite, List<Move> moves)
        {
            int[] dFiles = { -2, -2, -1, -1, 1, 1, 2, 2 };
            int[] dRanks = { -1, 1, -2, 2, -2, 2, -1, 1 };

            for (int i = 0; i < 8; i++)
            {
                int newFile = file + dFiles[i];
                int newRank = rank + dRanks[i];

                if (newFile >= 0 && newFile < 8 && newRank >= 0 && newRank < 8)
                {
                    int targetPiece = board.GetPiece(newFile, newRank);
                    if (targetPiece == Board.EMPTY || (targetPiece > 0) != isWhite)
                    {
                        moves.Add(new Move
                        {
                            From = $"{(char)('a' + file)}{rank + 1}",
                            To = $"{(char)('a' + newFile)}{newRank + 1}"
                        });
                    }
                }
            }
        }

        private void GenerateBishopMoves(int file, int rank, bool isWhite, List<Move> moves)
        {
            GenerateSlidingMoves(file, rank, isWhite, moves, true, false);
        }

        private void GenerateRookMoves(int file, int rank, bool isWhite, List<Move> moves)
        {
            GenerateSlidingMoves(file, rank, isWhite, moves, false, true);
        }

        private void GenerateQueenMoves(int file, int rank, bool isWhite, List<Move> moves)
        {
            GenerateSlidingMoves(file, rank, isWhite, moves, true, true);
        }

        private void GenerateSlidingMoves(int file, int rank, bool isWhite, List<Move> moves,
            bool diagonal, bool straight)
        {
            int[] dFiles = { -1, -1, -1, 0, 0, 1, 1, 1 };
            int[] dRanks = { -1, 0, 1, -1, 1, -1, 0, 1 };

            for (int dir = 0; dir < 8; dir++)
            {
                bool isDiagonal = (dFiles[dir] != 0 && dRanks[dir] != 0);
                if ((diagonal && isDiagonal) || (straight && !isDiagonal))
                {
                    for (int dist = 1; dist < 8; dist++)
                    {
                        int newFile = file + dFiles[dir] * dist;
                        int newRank = rank + dRanks[dir] * dist;

                        if (newFile < 0 || newFile >= 8 || newRank < 0 || newRank >= 8)
                            break;

                        int targetPiece = board.GetPiece(newFile, newRank);
                        if (targetPiece == Board.EMPTY)
                        {
                            moves.Add(new Move
                            {
                                From = $"{(char)('a' + file)}{rank + 1}",
                                To = $"{(char)('a' + newFile)}{newRank + 1}"
                            });
                        }
                        else
                        {
                            if ((targetPiece > 0) != isWhite)
                            {
                                moves.Add(new Move
                                {
                                    From = $"{(char)('a' + file)}{rank + 1}",
                                    To = $"{(char)('a' + newFile)}{newRank + 1}"
                                });
                            }
                            break;
                        }
                    }
                }
            }
        }

        private void GenerateKingMoves(int file, int rank, bool isWhite, List<Move> moves)
        {
            for (int df = -1; df <= 1; df++)
            {
                for (int dr = -1; dr <= 1; dr++)
                {
                    if (df == 0 && dr == 0) continue;

                    int newFile = file + df;
                    int newRank = rank + dr;

                    if (newFile >= 0 && newFile < 8 && newRank >= 0 && newRank < 8)
                    {
                        int targetPiece = board.GetPiece(newFile, newRank);
                        if (targetPiece == Board.EMPTY || (targetPiece > 0) != isWhite)
                        {
                            moves.Add(new Move
                            {
                                From = $"{(char)('a' + file)}{rank + 1}",
                                To = $"{(char)('a' + newFile)}{newRank + 1}"
                            });
                        }
                    }
                }
            }
        }

        private List<Move> FilterLegalMoves(List<Move> moves)
        {
            var legalMoves = new List<Move>();

            foreach (var move in moves)
            {
                // Proveri da li potez ostavlja kralja u šahu
                if (!LeavesKingInCheck(move))
                {
                    legalMoves.Add(move);
                }
            }

            return legalMoves;
        }

        private bool LeavesKingInCheck(Move move)
        {
            // Bez menjanja board-a, logički proveri
            string from = move.From;
            string to = move.To;

            int fromFile = from[0] - 'a';
            int fromRank = from[1] - '1';
            int toFile = to[0] - 'a';
            int toRank = to[1] - '1';

            int movingPiece = board.GetPiece(fromFile, fromRank);
            bool isWhiteToMove = board.IsWhiteToMove();

            // Ako je u šahu, mora da skloni kralja ili blokira
            if (IsKingInCheck(isWhiteToMove))
            {
                // Za sada, dozvoli samo poteze kralja
                if (Math.Abs(movingPiece) == Board.WHITE_KING)
                {
                    // Proveri da li kralj ide na sigurno polje
                    return WouldSquareBeAttacked(toFile, toRank, !isWhiteToMove, fromFile, fromRank);
                }
                // TODO: Dodaj logiku za blokiranje šaha
                return true; // Ostali potezi nisu dozvoljeni kad smo u šahu
            }

            // Ako pomeramo kralja, ne sme na napadnuto polje
            if (Math.Abs(movingPiece) == Board.WHITE_KING)
            {
                return WouldSquareBeAttacked(toFile, toRank, !isWhiteToMove, fromFile, fromRank);
            }

            // Za ostale figure, osnovne provere
            return false;
        }

        private bool WouldSquareBeAttacked(int targetFile, int targetRank, bool byWhite,
            int ignoredFile, int ignoredRank)
        {
            // Proveri da li bi polje bilo napadnuto, ignorišući figuru na ignoredFile/Rank
            for (int fromRank = 0; fromRank < 8; fromRank++)
            {
                for (int fromFile = 0; fromFile < 8; fromFile++)
                {
                    if (fromFile == ignoredFile && fromRank == ignoredRank) continue;

                    int piece = board.GetPiece(fromFile, fromRank);
                    if (piece == Board.EMPTY) continue;

                    bool isPieceWhite = piece > 0;
                    if (isPieceWhite != byWhite) continue;

                    if (CanPieceAttack(fromFile, fromRank, targetFile, targetRank, Math.Abs(piece)))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Brza evaluacija bez path analysis za debug
        /// </summary>
        public double QuickEvaluate()
        {
            double eval = 0.0;

            // Materijalna evaluacija
            for (int rank = 0; rank < 8; rank++)
            {
                for (int file = 0; file < 8; file++)
                {
                    int piece = board.GetPiece(file, rank);
                    if (piece != Board.EMPTY)
                    {
                        double value = GetPieceValue(Math.Abs(piece));
                        eval += (piece > 0) ? value : -value;
                    }
                }
            }

            return eval;
        }

        private bool IsSquareAttacked(int file, int rank, bool byWhite)
        {
            // Proveri sve protivničke figure
            for (int fromRank = 0; fromRank < 8; fromRank++)
            {
                for (int fromFile = 0; fromFile < 8; fromFile++)
                {
                    int piece = board.GetPiece(fromFile, fromRank);
                    if (piece == Board.EMPTY) continue;

                    bool isPieceWhite = piece > 0;
                    if (isPieceWhite != byWhite) continue;

                    // Debug
                    if (CanPieceAttack(fromFile, fromRank, file, rank, Math.Abs(piece)))
                    {
                        char pieceChar = GetPieceChar(piece);
                        Console.WriteLine($"info string Square {(char)('a' + file)}{rank + 1} is attacked by {pieceChar} at {(char)('a' + fromFile)}{fromRank + 1}");
                        return true;
                    }
                }
            }

            return false;
        }

        private char GetPieceChar(int piece)
        {
            int absPiece = Math.Abs(piece);
            char c = ' ';
            switch (absPiece)
            {
                case Board.WHITE_PAWN: c = 'P'; break;
                case Board.WHITE_KNIGHT: c = 'N'; break;
                case Board.WHITE_BISHOP: c = 'B'; break;
                case Board.WHITE_ROOK: c = 'R'; break;
                case Board.WHITE_QUEEN: c = 'Q'; break;
                case Board.WHITE_KING: c = 'K'; break;
            }
            return piece > 0 ? c : char.ToLower(c);
        }

        private bool CanPieceAttack(int fromFile, int fromRank, int toFile, int toRank, int pieceType)
        {
            int df = toFile - fromFile;
            int dr = toRank - fromRank;

            switch (pieceType)
            {
                case Board.WHITE_PAWN:
                    // Pawn attacks diagonally
                    bool isWhite = board.GetPiece(fromFile, fromRank) > 0;
                    int pawnDir = isWhite ? 1 : -1;
                    return Math.Abs(df) == 1 && dr == pawnDir;

                case Board.WHITE_KNIGHT:
                    return (Math.Abs(df) == 2 && Math.Abs(dr) == 1) ||
                           (Math.Abs(df) == 1 && Math.Abs(dr) == 2);

                case Board.WHITE_BISHOP:
                    if (Math.Abs(df) != Math.Abs(dr)) return false;
                    return IsPathClear(fromFile, fromRank, toFile, toRank);

                case Board.WHITE_ROOK:
                    if (df != 0 && dr != 0) return false;
                    return IsPathClear(fromFile, fromRank, toFile, toRank);

                case Board.WHITE_QUEEN:
                    if (df != 0 && dr != 0 && Math.Abs(df) != Math.Abs(dr)) return false;
                    return IsPathClear(fromFile, fromRank, toFile, toRank);

                case Board.WHITE_KING:
                    return Math.Abs(df) <= 1 && Math.Abs(dr) <= 1;

                default:
                    return false;
            }
        }

        private bool IsPathClear(int fromFile, int fromRank, int toFile, int toRank)
        {
            int df = toFile - fromFile;
            int dr = toRank - fromRank;
            int steps = Math.Max(Math.Abs(df), Math.Abs(dr));

            int stepFile = df == 0 ? 0 : df / Math.Abs(df);
            int stepRank = dr == 0 ? 0 : dr / Math.Abs(dr);

            for (int i = 1; i < steps; i++)
            {
                int checkFile = fromFile + i * stepFile;
                int checkRank = fromRank + i * stepRank;

                if (board.GetPiece(checkFile, checkRank) != Board.EMPTY)
                {
                    return false;
                }
            }

            return true;
        }

        // UCI protokol metode
        public void RunUCI()
        {
            // Ako UCIHandler očekuje SimpletonChessEngine, možemo kreirati wrapper
            Console.WriteLine("info string PathDependentEngine UCI mode not directly supported yet");
        }

        public void RunWinBoard()
        {
            // Ako WinBoardHandler očekuje SimpletonChessEngine, možemo kreirati wrapper
            Console.WriteLine("info string PathDependentEngine WinBoard mode not directly supported yet");
        }

        public async Task RunLichessBot()
        {
            // Placeholder implementation
            await Task.CompletedTask;
            Console.WriteLine("info string PathDependentEngine Lichess mode not implemented yet");
        }

        public void RunAutoDetect()
        {
            // Auto-detect implementation
            Console.WriteLine("PathDependentEngine Ready - Waiting for protocol detection...");
            // ... ostatak implementacije ...
        }

        public void NewGame()
        {
            gameState.Reset();
            board = gameState.GetBoard();
            constraintNetwork.Reset();
            pathMemory.Clear();
        }

        public void MakeMove(string move)
        {
            var parsedMove = Move.Parse(move);
            if (parsedMove != null)
            {
                gameState.MakeMove(parsedMove);
                constraintNetwork.UpdateAfterMove(parsedMove);
                pathMemory.RecordMove(parsedMove);
            }
        }

        public void SetPosition(string fen)
        {
            gameState.SetPosition(fen);
        }

        public void SetPosition(string[] moves)
        {
            NewGame();
            foreach (string move in moves)
            {
                MakeMove(move);
            }
        }
    }

    /// <summary>
    /// Constraint network za propagaciju ograničenja kroz game state
    /// </summary>
    public class ConstraintNetwork
    {
        private List<Constraint> constraints = new List<Constraint>();

        public List<Constraint> GetActiveConstraints(GameState state, Move move)
        {
            // Generiši constraints bazirane na trenutnoj poziciji
            var activeConstraints = new List<Constraint>();

            // Material constraints
            activeConstraints.Add(new MaterialConstraint(state));

            // Positional constraints
            activeConstraints.Add(new PositionalConstraint(state, move));

            // King safety constraints
            activeConstraints.Add(new KingSafetyConstraint(state));

            return activeConstraints;
        }

        public List<Constraint> PropagateConstraints(List<Constraint> current,
            Move move, double propagationRate)
        {
            var propagated = new List<Constraint>();

            foreach (var constraint in current)
            {
                var newConstraint = constraint.Propagate(move, propagationRate);
                if (newConstraint.Strength > 0.1) // Threshold
                {
                    propagated.Add(newConstraint);
                }
            }

            return propagated;
        }

        public void UpdateAfterMove(Move move)
        {
            // Update internal constraint state
        }

        public void Reset()
        {
            constraints.Clear();
        }
    }

    /// <summary>
    /// Abstract constraint klasa
    /// </summary>
    public abstract class Constraint
    {
        public double Strength { get; set; }
        public abstract Constraint Propagate(Move move, double rate);
    }

    /// <summary>
    /// Konkretne constraint implementacije
    /// </summary>
    public class MaterialConstraint : Constraint
    {
        private readonly GameState state;

        public MaterialConstraint(GameState state)
        {
            this.state = state;
            Strength = 1.0;
        }

        public override Constraint Propagate(Move move, double rate)
        {
            var newConstraint = new MaterialConstraint(state);
            newConstraint.Strength = this.Strength * rate;
            return newConstraint;
        }
    }

    public class PositionalConstraint : Constraint
    {
        private readonly GameState state;
        private readonly Move referenceMove;

        public PositionalConstraint(GameState state, Move move)
        {
            this.state = state;
            this.referenceMove = move;
            Strength = 0.8;
        }

        public override Constraint Propagate(Move move, double rate)
        {
            var newConstraint = new PositionalConstraint(state, move);
            newConstraint.Strength = this.Strength * rate;
            return newConstraint;
        }
    }

    public class KingSafetyConstraint : Constraint
    {
        private readonly GameState state;

        public KingSafetyConstraint(GameState state)
        {
            this.state = state;
            Strength = 0.9;
        }

        public override Constraint Propagate(Move move, double rate)
        {
            var newConstraint = new KingSafetyConstraint(state);
            newConstraint.Strength = this.Strength * rate * 1.1; // King safety se pojačava
            return newConstraint;
        }
    }

    /// <summary>
    /// Tensor probability space za path-dependent calculations
    /// </summary>
    public class TensorProbabilitySpace
    {
        public double ComputeTensorProbability(GameState state, Move move,
            List<Constraint> constraints, int pathIndex)
        {
            // Tensor calculation P_ij(A, π)
            double baseProbability = 0.5; // Starting point

            // Apply constraints to modify probability
            foreach (var constraint in constraints)
            {
                baseProbability *= (1.0 + constraint.Strength * 0.1);
            }

            // Path index affects probability (tensor indices)
            baseProbability *= Math.Exp(-pathIndex * 0.05);

            return Math.Min(1.0, Math.Max(0.0, baseProbability));
        }
    }

    /// <summary>
    /// Path memory za praćenje istorije
    /// </summary>
    public class PathMemory
    {
        private List<Move> moveHistory = new List<Move>();

        public void RecordMove(Move move)
        {
            moveHistory.Add(move);
        }

        public void Clear()
        {
            moveHistory.Clear();
        }

        public List<Move> GetRecentMoves(int count)
        {
            return moveHistory.TakeLast(count).ToList();
        }
    }
}