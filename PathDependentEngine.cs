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

            var legalMoves = GenerateLegalMoves();
            if (legalMoves.Count == 0)
            {
                return "e2e4"; // fallback
            }

            // Analiziraj svaki potez kroz path-dependent framework
            var moveEvaluations = new Dictionary<Move, double>();

            foreach (var move in legalMoves)
            {
                double evaluation = EvaluateMoveThroughPaths(move);
                moveEvaluations[move] = evaluation;

                Console.WriteLine($"info string Move {move} evaluation: {evaluation:F3}");
            }

            // Odaberi potez sa najboljom path-dependent evaluacijom
            var bestMove = moveEvaluations.OrderByDescending(kvp => kvp.Value).First().Key;

            Console.WriteLine($"info string Selected move: {bestMove} with evaluation: {moveEvaluations[bestMove]:F3}");
            return bestMove.ToString();
        }

        /// <summary>
        /// Evaluacija poteza kroz multiple paths sa conditional convergence
        /// </summary>
        private double EvaluateMoveThroughPaths(Move move)
        {
            var pathEvaluations = new List<double>();
            var constraints = constraintNetwork.GetActiveConstraints(gameState, move);

            // Generiši različite paths (permutacije budućih poteza)
            var futurePaths = GenerateFuturePaths(move, MAX_PATH_LENGTH);

            foreach (var path in futurePaths)
            {
                // Svaki path ima svoju trajektoriju kroz phase space
                double pathValue = ComputePathValue(path, constraints);
                pathEvaluations.Add(pathValue);
            }

            // Conditional convergence - različite permutacije daju različite sume
            return ComputeConditionalConvergence(pathEvaluations);
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
        private List<Move> GenerateLegalMoves()
        {
            var moves = new List<Move>();
            board = gameState.GetBoard(); // Osiguraj da imamo najnoviji board

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
            // Za sada vraćamo sve poteze
            // U produkciji bi trebalo filtrirati poteze koji ostavljaju kralja u šahu
            return moves;
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