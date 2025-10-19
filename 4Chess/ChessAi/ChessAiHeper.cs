using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _4Chess.Game;
using _4Chess.Pieces;

namespace _4Chess.ChessAi
{
    public static class ChessAiHelper
    {
        /// <summary>
        /// Konvertiert das Board in FEN-Notation (Forsyth-Edwards Notation)
        /// </summary>
        public static string BoardToFEN(List<List<Piece?>> board, bool isWhiteTurn, int moveCounter)
        {
            StringBuilder fen = new StringBuilder();

            // 1. Piece placement
            for (int y = 0; y < 8; y++)
            {
                int emptyCount = 0;
                for (int x = 0; x < 8; x++)
                {
                    var piece = board[y][x];
                    if (piece == null)
                    {
                        emptyCount++;
                    }
                    else
                    {
                        if (emptyCount > 0)
                        {
                            fen.Append(emptyCount);
                            emptyCount = 0;
                        }
                        fen.Append(PieceToFENChar(piece));
                    }
                }
                if (emptyCount > 0)
                {
                    fen.Append(emptyCount);
                }
                if (y < 7)
                {
                    fen.Append('/');
                }
            }

            // 2. Active color
            fen.Append(isWhiteTurn ? " w " : " b ");

            // 3. Castling availability
            string castling = GetCastlingRights(board);
            fen.Append(castling);
            fen.Append(' ');

            // 4. En passant target square
            string enPassant = GetEnPassantSquare(board);
            fen.Append(enPassant);
            fen.Append(' ');

            // 5. Halfmove clock (simplified, always 0 for now)
            fen.Append("0 ");

            // 6. Fullmove number
            fen.Append(moveCounter);

            return fen.ToString();
        }

        /// <summary>
        /// Konvertiert eine Figur in das FEN-Zeichen
        /// </summary>
        private static char PieceToFENChar(Piece piece)
        {
            char c = piece switch
            {
                Pawn => 'p',
                Knight => 'n',
                Bishop => 'b',
                Rook => 'r',
                Queen => 'q',
                King => 'k',
                _ => '?'
            };

            return piece.Alignment == Piece.Color.White ? char.ToUpper(c) : c;
        }

        /// <summary>
        /// Ermittelt die Rochade-Rechte
        /// </summary>
        private static string GetCastlingRights(List<List<Piece?>> board)
        {
            StringBuilder rights = new StringBuilder();

            // White King-side
            if (board[7][4] is King whiteKing && whiteKing.IsUnmoved &&
                board[7][7] is Rook whiteRookKingSide && whiteRookKingSide.IsUnmoved)
            {
                rights.Append('K');
            }

            // White Queen-side
            if (board[7][4] is King whiteKingQ && whiteKingQ.IsUnmoved &&
                board[7][0] is Rook whiteRookQueenSide && whiteRookQueenSide.IsUnmoved)
            {
                rights.Append('Q');
            }

            // Black King-side
            if (board[0][4] is King blackKing && blackKing.IsUnmoved &&
                board[0][7] is Rook blackRookKingSide && blackRookKingSide.IsUnmoved)
            {
                rights.Append('k');
            }

            // Black Queen-side
            if (board[0][4] is King blackKingQ && blackKingQ.IsUnmoved &&
                board[0][0] is Rook blackRookQueenSide && blackRookQueenSide.IsUnmoved)
            {
                rights.Append('q');
            }

            return rights.Length > 0 ? rights.ToString() : "-";
        }

        /// <summary>
        /// Ermittelt das En Passant Zielfeld
        /// </summary>
        private static string GetEnPassantSquare(List<List<Piece?>> board)
        {
            // Suche nach Bauern mit IsEnPassant flag
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    if (board[y][x] is Pawn pawn && pawn.IsEnPassant)
                    {
                        // Das Zielfeld ist hinter dem Bauern
                        int targetY = pawn.Alignment == Piece.Color.White ? y + 1 : y - 1;
                        return SquareToUCI(x, targetY);
                    }
                }
            }
            return "-";
        }

        /// <summary>
        /// Konvertiert Board-Koordinaten in UCI-Notation (z.B. e2, e4)
        /// </summary>
        public static string SquareToUCI(int x, int y)
        {
            char file = (char)('a' + x);
            char rank = (char)('1' + (7 - y));
            return $"{file}{rank}";
        }

        /// <summary>
        /// Konvertiert UCI-Notation in Board-Koordinaten
        /// </summary>
        public static (int x, int y) UCIToSquare(string uci)
        {
            int x = uci[0] - 'a';
            int y = 7 - (uci[1] - '1');
            return (x, y);
        }

        /// <summary>
        /// Konvertiert einen UCI-Move (z.B. "e2e4") in Start- und Zielkoordinaten
        /// </summary>
        public static ((int x, int y) from, (int x, int y) to, char promotion) ParseUCIMove(string uciMove)
        {
            var from = UCIToSquare(uciMove.Substring(0, 2));
            var to = UCIToSquare(uciMove.Substring(2, 2));
            char promotion = uciMove.Length > 4 ? uciMove[4] : '\0';
            return (from, to, promotion);
        }

        /// <summary>
        /// Erstellt einen UCI-Move aus Koordinaten
        /// </summary>
        public static string MoveToUCI(int fromX, int fromY, int toX, int toY, char promotion = '\0')
        {
            string move = SquareToUCI(fromX, fromY) + SquareToUCI(toX, toY);
            if (promotion != '\0')
            {
                move += char.ToLower(promotion);
            }
            return move;
        }

        /// <summary>
        /// Konvertiert alle bisherigen Züge in UCI-Notation
        /// </summary>
        public static string GetMovesHistory(List<string> moves)
        {
            if (moves.Count == 0)
                return "startpos";

            return "startpos moves " + string.Join(" ", moves);
        }
    }

    public enum ChessAiDifficulty
    {
        Low,
        Medium,
        High,
        Ultra,
        UltraPlus,
        Overthinker
    }
}
