using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpletonChessEngine
{
    // Kompletna implementacija board reprezentacije
    public class Board
    {
        public enum PieceType
        {
            Empty = 0,
            Pawn = 1,
            Rook = 2,
            Knight = 3,
            Bishop = 4,
            Queen = 5,
            King = 6
        }


        private int[,] squares = new int[8, 8];
        private bool whiteToMove = true;
        private bool whiteCanCastleKingside = true;
        private bool whiteCanCastleQueenside = true;
        private bool blackCanCastleKingside = true;
        private bool blackCanCastleQueenside = true;
        private string enPassantSquare = "";

        // En passant
        private int enPassantFile = -1; // -1 means no en passant possible

        // Move counters
        private int halfmoveClock = 0;    // Half-moves since last pawn move or capture
        private int fullmoveNumber = 1;   // Increments after Black's move

        // Piece constants
        public const int EMPTY = 0;
        public const int WHITE_PAWN = 1;
        public const int WHITE_ROOK = 2;
        public const int WHITE_KNIGHT = 3;
        public const int WHITE_BISHOP = 4;
        public const int WHITE_QUEEN = 5;
        public const int WHITE_KING = 6;
        public const int BLACK_PAWN = -1;
        public const int BLACK_ROOK = -2;
        public const int BLACK_KNIGHT = -3;
        public const int BLACK_BISHOP = -4;
        public const int BLACK_QUEEN = -5;
        public const int BLACK_KING = -6;

        public bool WhiteCanCastleKingside { get => whiteCanCastleKingside; set => whiteCanCastleKingside = value; }
        public bool WhiteCanCastleQueenside { get => whiteCanCastleQueenside; set => whiteCanCastleQueenside = value; }
        public bool BlackCanCastleQueenside { get => blackCanCastleQueenside; set => blackCanCastleQueenside = value; }
        public int[,] Squares { get => squares; set => squares = value; }
        public bool BlackCanCastleKingside { get => blackCanCastleKingside; set => blackCanCastleKingside = value; }

        public Board()
        {
            SetupStartingPosition();
        }

        public Board(Board other)
        {
            Array.Copy(other.Squares, this.Squares, 64);

            for (int rank = 0; rank < 8; rank++)
            {
                for (int file = 0; file < 8; file++)
                {
                    this.Squares[file, rank] = other.Squares[file, rank];
                }
            }

            this.whiteToMove = other.whiteToMove;
            this.WhiteCanCastleKingside = other.WhiteCanCastleKingside;
            this.WhiteCanCastleQueenside = other.WhiteCanCastleQueenside;
            this.BlackCanCastleKingside = other.BlackCanCastleKingside;
            this.BlackCanCastleQueenside = other.BlackCanCastleQueenside;
            this.enPassantSquare = other.enPassantSquare;
        }

        private void SetupStartingPosition()
        {
            // Clear board
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                    Squares[i, j] = EMPTY;

            // White pieces
            Squares[0, 0] = WHITE_ROOK; Squares[0, 7] = WHITE_ROOK;
            Squares[0, 1] = WHITE_KNIGHT; Squares[0, 6] = WHITE_KNIGHT;
            Squares[0, 2] = WHITE_BISHOP; Squares[0, 5] = WHITE_BISHOP;
            Squares[0, 3] = WHITE_QUEEN; Squares[0, 4] = WHITE_KING;
            for (int i = 0; i < 8; i++) Squares[1, i] = WHITE_PAWN;

            // Black pieces
            Squares[7, 0] = BLACK_ROOK; Squares[7, 7] = BLACK_ROOK;
            Squares[7, 1] = BLACK_KNIGHT; Squares[7, 6] = BLACK_KNIGHT;
            Squares[7, 2] = BLACK_BISHOP; Squares[7, 5] = BLACK_BISHOP;
            Squares[7, 3] = BLACK_QUEEN; Squares[7, 4] = BLACK_KING;
            for (int i = 0; i < 8; i++) Squares[6, i] = BLACK_PAWN;
        }

        public int GetPiece(int file, int rank)
        {
            if (file < 0 || file > 7 || rank < 0 || rank > 7) return EMPTY;
            return Squares[rank, file];
        }

        public void SetPiece(int file, int rank, int piece)
        {
            if (file >= 0 && file <= 7 && rank >= 0 && rank <= 7)
                Squares[rank, file] = piece;
        }

        public bool IsWhiteToMove() => whiteToMove;

        public void MakeMove(ChessMove move)
        {
            int piece = GetPiece(move.FromFile, move.FromRank);
            int capturedPiece = GetPiece(move.ToFile, move.ToRank);

            // Clear source square
            SetPiece(move.FromFile, move.FromRank, EMPTY);

            // Set destination square (with promotion if applicable)
            SetPiece(move.ToFile, move.ToRank, move.PromotionPiece != EMPTY ? move.PromotionPiece : piece);

            // Handle castling
            if (Math.Abs(piece) == Math.Abs(WHITE_KING) && Math.Abs(move.FromFile - move.ToFile) == 2)
            {
                if (move.ToFile == 6) // Kingside
                {
                    int rook = GetPiece(7, move.FromRank);
                    SetPiece(7, move.FromRank, EMPTY);
                    SetPiece(5, move.FromRank, rook);
                }
                else if (move.ToFile == 2) // Queenside
                {
                    int rook = GetPiece(0, move.FromRank);
                    SetPiece(0, move.FromRank, EMPTY);
                    SetPiece(3, move.FromRank, rook);
                }
            }

            // Handle en passant capture
            if (Math.Abs(piece) == Math.Abs(WHITE_PAWN) && move.ToFile != move.FromFile && capturedPiece == EMPTY)
            {
                // En passant capture - remove the captured pawn
                int capturedPawnRank = piece > 0 ? move.ToRank - 1 : move.ToRank + 1;
                SetPiece(move.ToFile, capturedPawnRank, EMPTY);
            }

            // Update en passant file for next move
            if (Math.Abs(piece) == Math.Abs(WHITE_PAWN) && Math.Abs(move.ToRank - move.FromRank) == 2)
            {
                // Pawn moved two squares - set en passant file
                enPassantFile = move.FromFile;
            }
            else
            {
                enPassantFile = -1; // Clear en passant
            }

            // Update castling rights
            if (piece == WHITE_KING)
            {
                WhiteCanCastleKingside = false;
                WhiteCanCastleQueenside = false;
            }
            else if (piece == BLACK_KING)
            {
                BlackCanCastleKingside = false;
                BlackCanCastleQueenside = false;
            }
            else if (piece == WHITE_ROOK)
            {
                if (move.FromFile == 0 && move.FromRank == 0) WhiteCanCastleQueenside = false;
                if (move.FromFile == 7 && move.FromRank == 0) WhiteCanCastleKingside = false;
            }
            else if (piece == BLACK_ROOK)
            {
                if (move.FromFile == 0 && move.FromRank == 7) BlackCanCastleQueenside = false;
                if (move.FromFile == 7 && move.FromRank == 7) BlackCanCastleKingside = false;
            }

            // Also lose castling rights if rook is captured
            if (capturedPiece == WHITE_ROOK)
            {
                if (move.ToFile == 0 && move.ToRank == 0) WhiteCanCastleQueenside = false;
                if (move.ToFile == 7 && move.ToRank == 0) WhiteCanCastleKingside = false;
            }
            else if (capturedPiece == BLACK_ROOK)
            {
                if (move.ToFile == 0 && move.ToRank == 7) BlackCanCastleQueenside = false;
                if (move.ToFile == 7 && move.ToRank == 7) BlackCanCastleKingside = false;
            }

            // Update move counters
            if (Math.Abs(piece) == Math.Abs(WHITE_PAWN) || capturedPiece != EMPTY)
            {
                halfmoveClock = 0; // Reset on pawn move or capture
            }
            else
            {
                halfmoveClock++;
            }

            if (!whiteToMove) // After black's move
            {
                fullmoveNumber++;
            }

            // Switch turns
            whiteToMove = !whiteToMove;
        }

        public List<ChessMove> GenerateLegalMoves()
        {
            var pseudoLegalMoves = GeneratePseudoLegalMoves();
            var legalMoves = new List<ChessMove>();

            foreach (var move in pseudoLegalMoves)
            {
                var testBoard = new Board(this);
                testBoard.MakeMove(move);
                bool kingInCheck = testBoard.IsInCheck(this.whiteToMove);

                if (!kingInCheck)
                {
                    legalMoves.Add(move);
                }
            }

            return legalMoves;
        }

        public void UndoMove(ChessMove move)
        {
            // Ovo je MNOGO brže od kopiranja celog board-a
            // Za sada, koristi jednostavnu implementaciju:

            // Vrati potez
            int piece = GetPiece(move.ToFile, move.ToRank);
            SetPiece(move.FromFile, move.FromRank, piece);
            SetPiece(move.ToFile, move.ToRank, EMPTY); // Ovde treba capturedPiece

            // Vrati turn
            whiteToMove = !whiteToMove;

            // NAPOMENA: Ovo je jednostavna verzija
            // Pravi undo bi trebao da vrati castling rights, en passant, itd.
        }

        public bool IsInCheck(bool whiteKing)
        {
            // Nađi poziciju kralja
            int kingFile = -1, kingRank = -1;
            int targetKing = whiteKing ? WHITE_KING : BLACK_KING;

            for (int rank = 0; rank < 8; rank++)
            {
                for (int file = 0; file < 8; file++)
                {
                    if (GetPiece(file, rank) == targetKing)
                    {
                        kingFile = file;
                        kingRank = rank;
                        break;
                    }
                }
                if (kingFile != -1) break;
            }

            if (kingFile == -1) return false;


            return IsSquareAttackedBy(!whiteKing, kingFile, kingRank);
        }

        public bool IsSquareAttackedBy(bool byWhite, int targetFile, int targetRank)
        {
            // Bez debug output-a za brzinu
            for (int rank = 0; rank < 8; rank++)
            {
                for (int file = 0; file < 8; file++)
                {
                    int piece = GetPiece(file, rank);
                    if (piece == EMPTY) continue;

                    bool isPieceWhite = piece > 0;
                    if (isPieceWhite != byWhite) continue;

                    PieceType pieceType = (PieceType)Math.Abs(piece);

                    if (CanPieceAttackSquare(pieceType, file, rank, targetFile, targetRank))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void GeneratePawnMoves(int file, int rank, List<ChessMove> moves)
        {
            int piece = GetPiece(file, rank);
            int direction = piece > 0 ? 1 : -1;
            int startRank = piece > 0 ? 1 : 6;
            int promotionRank = piece > 0 ? 7 : 0;

            // Forward move
            if (GetPiece(file, rank + direction) == EMPTY)
            {
                if (rank + direction == promotionRank)
                {
                    // Promotion
                    int queenType = piece > 0 ? WHITE_QUEEN : BLACK_QUEEN;
                    moves.Add(new ChessMove(file, rank, file, rank + direction, queenType));
                }
                else
                {
                    moves.Add(new ChessMove(file, rank, file, rank + direction));

                    // Two squares from start
                    if (rank == startRank && GetPiece(file, rank + 2 * direction) == EMPTY)
                    {
                        moves.Add(new ChessMove(file, rank, file, rank + 2 * direction));
                    }
                }
            }

            // Captures
            for (int df = -1; df <= 1; df += 2)
            {
                int targetFile = file + df;
                int targetRank = rank + direction;

                if (targetFile < 0 || targetFile > 7 || targetRank < 0 || targetRank > 7) continue;

                int target = GetPiece(targetFile, targetRank);
                if (target != EMPTY && (target > 0) != (piece > 0))
                {
                    if (targetRank == promotionRank)
                    {
                        int queenType = piece > 0 ? WHITE_QUEEN : BLACK_QUEEN;
                        moves.Add(new ChessMove(file, rank, targetFile, targetRank, queenType));
                    }
                    else
                    {
                        moves.Add(new ChessMove(file, rank, targetFile, targetRank));
                    }
                }
            }
        }

        private void GenerateRookMoves(int file, int rank, List<ChessMove> moves)
        {
            GenerateSlidingMoves(file, rank, moves, new int[,] { { 0, 1 }, { 0, -1 }, { 1, 0 }, { -1, 0 } });
        }

        private void GenerateBishopMoves(int file, int rank, List<ChessMove> moves)
        {
            GenerateSlidingMoves(file, rank, moves, new int[,] { { 1, 1 }, { 1, -1 }, { -1, 1 }, { -1, -1 } });
        }

        private void GenerateQueenMoves(int file, int rank, List<ChessMove> moves)
        {
            GenerateSlidingMoves(file, rank, moves, new int[,] { { 0, 1 }, { 0, -1 }, { 1, 0 }, { -1, 0 }, { 1, 1 }, { 1, -1 }, { -1, 1 }, { -1, -1 } });
        }

        private void GenerateSlidingMoves(int file, int rank, List<ChessMove> moves, int[,] directions)
        {
            int piece = GetPiece(file, rank);

            for (int d = 0; d < directions.GetLength(0); d++)
            {
                int df = directions[d, 0];
                int dr = directions[d, 1];

                for (int i = 1; i < 8; i++)
                {
                    int newFile = file + i * df;
                    int newRank = rank + i * dr;


                    if (newFile < 0 || newFile > 7 || newRank < 0 || newRank > 7)
                    {
                        break;
                    }

                    int target = GetPiece(newFile, newRank);

                    if (target == EMPTY)
                    {
                        moves.Add(new ChessMove(file, rank, newFile, newRank));
                    }
                    else
                    {
                        bool targetIsWhite = target > 0;
                        bool pieceIsWhite = piece > 0;

                        if (targetIsWhite != pieceIsWhite) // Capture
                        {
                            moves.Add(new ChessMove(file, rank, newFile, newRank));
                        }
                        //else
                        //{
                        //    Console.WriteLine($"info string Blocked by own piece");
                        //}

                        break;
                    }
                }
            }
        }

        private void GenerateKnightMoves(int file, int rank, List<ChessMove> moves)
        {
            int piece = GetPiece(file, rank);
            int[,] knightMoves = { { 2, 1 }, { 2, -1 }, { -2, 1 }, { -2, -1 }, { 1, 2 }, { 1, -2 }, { -1, 2 }, { -1, -2 } };

            for (int i = 0; i < 8; i++)
            {
                int newFile = file + knightMoves[i, 0];
                int newRank = rank + knightMoves[i, 1];

                if (newFile < 0 || newFile > 7 || newRank < 0 || newRank > 7) continue;

                int target = GetPiece(newFile, newRank);
                if (target == EMPTY || (target > 0) != (piece > 0))
                {
                    moves.Add(new ChessMove(file, rank, newFile, newRank));
                }
            }
        }

        private void GenerateKingMoves(int file, int rank, List<ChessMove> moves)
        {
            int piece = GetPiece(file, rank);

            // Normal king moves
            for (int df = -1; df <= 1; df++)
            {
                for (int dr = -1; dr <= 1; dr++)
                {
                    if (df == 0 && dr == 0) continue;
                    int newFile = file + df;
                    int newRank = rank + dr;
                    if (newFile < 0 || newFile > 7 || newRank < 0 || newRank > 7) continue;

                    int target = GetPiece(newFile, newRank);
                    if (target == EMPTY || (target > 0) != (piece > 0))
                    {
                        // Ne testiraj da li je polje napadano ovde - to radi GenerateLegalMoves
                        moves.Add(new ChessMove(file, rank, newFile, newRank));
                    }
                }
            }

            // Castling - dodaj debug
            bool isWhite = piece > 0;

            if (IsInCheck(isWhite))
            {
                return;
            }

            // White castling
            if (piece == WHITE_KING && rank == 0 && file == 4)
            {

                // Kingside castling (e1-g1)
                if (WhiteCanCastleKingside)
                {
                    if (GetPiece(5, 0) == EMPTY &&
                        GetPiece(6, 0) == EMPTY &&
                        !IsSquareAttackedBy(false, 5, 0) &&
                        !IsSquareAttackedBy(false, 6, 0))
                    {
                        Console.WriteLine("info string Adding white kingside castling: e1g1");
                        moves.Add(new ChessMove(4, 0, 6, 0));
                    }
                }

                // Queenside castling (e1-c1)
                if (WhiteCanCastleQueenside)
                {
                    if (GetPiece(3, 0) == EMPTY &&
                        GetPiece(2, 0) == EMPTY &&
                        GetPiece(1, 0) == EMPTY &&
                        !IsSquareAttackedBy(false, 3, 0) &&
                        !IsSquareAttackedBy(false, 2, 0))
                    {
                        moves.Add(new ChessMove(4, 0, 2, 0));
                    }
                }
            }
            // Black castling
            else if (piece == BLACK_KING && rank == 7 && file == 4)
            {
                // Kingside castling
                if (BlackCanCastleKingside &&
                    GetPiece(5, 7) == EMPTY &&
                    GetPiece(6, 7) == EMPTY &&
                    !IsSquareAttackedBy(true, 5, 7) &&
                    !IsSquareAttackedBy(true, 6, 7))
                {
                    moves.Add(new ChessMove(4, 7, 6, 7));
                }

                // Queenside castling
                if (BlackCanCastleQueenside &&
                    GetPiece(3, 7) == EMPTY &&
                    GetPiece(2, 7) == EMPTY &&
                    GetPiece(1, 7) == EMPTY &&
                    !IsSquareAttackedBy(true, 3, 7) &&
                    !IsSquareAttackedBy(true, 2, 7))
                {
                    moves.Add(new ChessMove(4, 7, 2, 7));
                }
            }
        }

        private bool IsLegalMove(ChessMove move)
        {
            // ZA SADA SAMO VRATI TRUE - GUI šalje legalne poteze
            return true;
        }

        private bool CanPieceAttackSquare(PieceType pieceType, int fromFile, int fromRank, int toFile, int toRank)
        {
            int deltaFile = toFile - fromFile;
            int deltaRank = toRank - fromRank;

            switch (pieceType)
            {
                case PieceType.Pawn:
                    // Pawn attacks diagonally
                    int pawnDirection = GetPiece(fromFile, fromRank) > 0 ? 1 : -1; // White goes up, black goes down
                    return Math.Abs(deltaFile) == 1 && deltaRank == pawnDirection;

                case PieceType.Rook:
                    // Rook attacks horizontally or vertically
                    if (deltaFile == 0 || deltaRank == 0)
                    {
                        return IsPathClear(fromFile, fromRank, toFile, toRank);
                    }
                    return false;

                case PieceType.Bishop:
                    // Bishop attacks diagonally
                    if (Math.Abs(deltaFile) == Math.Abs(deltaRank))
                    {
                        return IsPathClear(fromFile, fromRank, toFile, toRank);
                    }
                    return false;

                case PieceType.Queen:
                    // Queen combines rook and bishop
                    if (deltaFile == 0 || deltaRank == 0 || Math.Abs(deltaFile) == Math.Abs(deltaRank))
                    {
                        return IsPathClear(fromFile, fromRank, toFile, toRank);
                    }
                    return false;

                case PieceType.Knight:
                    // Knight moves in L-shape
                    return (Math.Abs(deltaFile) == 2 && Math.Abs(deltaRank) == 1) ||
                           (Math.Abs(deltaFile) == 1 && Math.Abs(deltaRank) == 2);

                case PieceType.King:
                    // King attacks adjacent squares
                    return Math.Abs(deltaFile) <= 1 && Math.Abs(deltaRank) <= 1 && (deltaFile != 0 || deltaRank != 0);
            }

            return false;
        }

        private bool IsPathClear(int fromFile, int fromRank, int toFile, int toRank)
        {
            int deltaFile = toFile - fromFile;
            int deltaRank = toRank - fromRank;

            int stepFile = deltaFile == 0 ? 0 : (deltaFile > 0 ? 1 : -1);
            int stepRank = deltaRank == 0 ? 0 : (deltaRank > 0 ? 1 : -1);

            int currentFile = fromFile + stepFile;
            int currentRank = fromRank + stepRank;

            while (currentFile != toFile || currentRank != toRank)
            {
                if (GetPiece(currentFile, currentRank) != EMPTY)
                {
                    return false; // Path is blocked
                }

                currentFile += stepFile;
                currentRank += stepRank;
            }

            return true;
        }




        public List<ChessMove> GeneratePseudoLegalMoves()
        {
            var moves = new List<ChessMove>();

            for (int rank = 0; rank < 8; rank++)
            {
                for (int file = 0; file < 8; file++)
                {
                    int piece = GetPiece(file, rank);
                    if (piece == EMPTY) continue;

                    bool pieceIsWhite = piece > 0;
                    if (pieceIsWhite != whiteToMove) continue;

                    PieceType pieceType = (PieceType)Math.Abs(piece);

                    int moveCountBefore = moves.Count;

                    switch (pieceType)
                    {
                        case PieceType.Pawn:
                            GeneratePawnMoves(file, rank, moves);
                            break;
                        case PieceType.Rook:
                            GenerateRookMoves(file, rank, moves);
                            break;
                        case PieceType.Knight:
                            GenerateKnightMoves(file, rank, moves);
                            break;
                        case PieceType.Bishop:
                            GenerateBishopMoves(file, rank, moves);
                            break;
                        case PieceType.Queen:
                            GenerateQueenMoves(file, rank, moves);
                            break;
                        case PieceType.King:
                            GenerateKingMoves(file, rank, moves);
                            break;
                    }

                    int moveCountAfter = moves.Count;
                }
            }

            return moves;
        }

        public bool IsCheckmate()
        {
            return IsInCheck(whiteToMove) && GenerateLegalMoves().Count == 0;
        }

        public bool IsStalemate()
        {
            return !IsInCheck(whiteToMove) && GenerateLegalMoves().Count == 0;
        }

        public int Evaluate()
        {
            int score = 0;

            // Material values
            int[] pieceValues = { 0, 100, 500, 300, 300, 900, 20000 }; // Pawn, Rook, Knight, Bishop, Queen, King

            for (int rank = 0; rank < 8; rank++)
            {
                for (int file = 0; file < 8; file++)
                {
                    int piece = GetPiece(file, rank);
                    if (piece != EMPTY)
                    {
                        int value = pieceValues[Math.Abs(piece)];
                        score += piece > 0 ? value : -value;
                    }
                }
            }

            return score;
        }
    }
}