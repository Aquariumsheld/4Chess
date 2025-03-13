using _4Chess.Pieces;
using System.Linq;
using System.Numerics;

namespace _4Chess.Game
{
    public class MoveCounter
    {
        // Globaler Wert: Gibt an, wie viele Züge (Zug-Tiefe) ins Voraus simuliert werden sollen.
        public static int MovesAhead = 1;

        /// <summary>
        /// Zählt die rohen (nicht simulierten) möglichen Züge aller Figuren.
        /// </summary>
        public int CountTotalMoves(_4ChessGame game)
        {
            int whiteMoves = 0;
            int blackMoves = 0;

            foreach (var row in game.Board)
            {
                foreach (var piece in row)
                {
                    if (piece == null)
                        continue;

                    var moves = piece.GetMoves();
                    if (piece.Alignment == Piece.Color.White)
                        whiteMoves += moves.Count;
                    else if (piece.Alignment == Piece.Color.Black)
                        blackMoves += moves.Count;
                }
            }

            return whiteMoves + blackMoves;
        }

        /// <summary>
        /// Zählt die legalen Züge im aktuellen Zustand.
        /// </summary>
        public int CountTotalLegalMoves(_4ChessGame game)
        {
            int totalLegalMoves = 0;

            foreach (var row in game.Board)
            {
                for(int i = 0; i < row.Count; i++)
                {
                    var piece = row[i];
                    if (piece == null)
                        continue;
                    var moves = piece.GetMoves();
                    foreach (var move in moves)
                    {
                        if (IsLegalMove(game, piece, move))
                        {
                            totalLegalMoves++;
                        }
                    }
                }
            }
            return totalLegalMoves;
        }

        /// <summary>
        /// Rekursive Methode zur Zählung der Board-Varianten (unterschiedliche Zustände),
        /// basierend auf der Simulation von legalen Zügen bis zur angegebenen Tiefe.
        /// </summary>
        public long CountBoardVariants(_4ChessGame game, int depth)
        {
            if (depth == 0)
                return 1;

            long variants = 0;

            // Für jede Figur auf dem Board
            foreach (var row in game.Board)
            {
                for(int i = 0; i < row.Count; i++)
                {
                    var piece = row[i];
                    if (piece == null)
                        continue;
                    var moves = piece.GetMoves();
                    foreach (var move in moves)
                    {
                        // Nur wenn der Zug legal ist, wird simuliert.
                        if (IsLegalMove(game, piece, move))
                        {
                            int origX = piece.X;
                            int origY = piece.Y;
                            Piece targetPiece = game.Board[(int)move.Y][(int)move.X];
                            Vector2 originalKingPos = new Vector2();
                            if (piece is King)
                            {
                                originalKingPos = piece.Alignment == Piece.Color.White
                                    ? game.WhiteKingPosition
                                    : game.BlackKingPosition;
                            }
                            // Zug simulieren: Auf dem Board wird der Zug durchgeführt.
                            game.Board[origY][origX] = null;
                            piece.X = (int)move.X;
                            piece.Y = (int)move.Y;
                            game.Board[piece.Y][piece.X] = piece;
                            if (piece is King)
                            {
                                if (piece.Alignment == Piece.Color.White)
                                    game.WhiteKingPosition = new Vector2(piece.X, piece.Y);
                                else
                                    game.BlackKingPosition = new Vector2(piece.X, piece.Y);
                            }
                            // Rekursiv die Varianten in der Tiefe - 1 zählen
                            long subVariants = CountBoardVariants(game, depth - 1);
                            variants += subVariants;
                            // Den simulierten Zug rückgängig machen (Undo)
                            game.Board[piece.Y][piece.X] = targetPiece;
                            game.Board[origY][origX] = piece;
                            piece.X = origX;
                            piece.Y = origY;
                            if (piece is King)
                            {
                                if (piece.Alignment == Piece.Color.White)
                                    game.WhiteKingPosition = originalKingPos;
                                else
                                    game.BlackKingPosition = originalKingPos;
                            }
                        }
                    }
                }
            }
            return variants;
        }

        /// <summary>
        /// Simuliert einen Zug und prüft, ob der eigene König danach nicht im Schach steht.
        /// </summary>
        private bool IsLegalMove(_4ChessGame game, Piece piece, Vector2 move)
        {
            int origX = piece.X;
            int origY = piece.Y;
            Piece targetPiece = game.Board[(int)move.Y][(int)move.X];
            Vector2 originalKingPos = new Vector2();
            if (piece is King)
            {
                originalKingPos = piece.Alignment == Piece.Color.White
                    ? game.WhiteKingPosition
                    : game.BlackKingPosition;
            }

            // Zug simulieren
            game.Board[origY][origX] = null;
            piece.X = (int)move.X;
            piece.Y = (int)move.Y;
            game.Board[piece.Y][piece.X] = piece;

            if (piece is King)
            {
                if (piece.Alignment == Piece.Color.White)
                    game.WhiteKingPosition = new Vector2(piece.X, piece.Y);
                else
                    game.BlackKingPosition = new Vector2(piece.X, piece.Y);
            }

            bool legal = !IsKingInCheck(game, piece.Alignment);

            // Undo: Zustand wiederherstellen
            game.Board[piece.Y][piece.X] = targetPiece;
            game.Board[origY][origX] = piece;
            piece.X = origX;
            piece.Y = origY;

            if (piece is King)
            {
                if (piece.Alignment == Piece.Color.White)
                    game.WhiteKingPosition = originalKingPos;
                else
                    game.BlackKingPosition = originalKingPos;
            }

            return legal;
        }

        /// <summary>
        /// Prüft, ob der König der angegebenen Farbe im aktuellen Zustand in Schach steht.
        /// </summary>
        private bool IsKingInCheck(_4ChessGame game, Piece.Color kingColor)
        {
            Vector2 kingPos = kingColor == Piece.Color.White
                ? game.WhiteKingPosition
                : game.BlackKingPosition;

            foreach (var row in game.Board)
            {
                for (int i = 0; i < row.Count; i++)
                {
                    var enemy = row[i];
                    if (enemy == null)
                        continue;
                    if (enemy.Alignment == kingColor)
                        continue;

                    var enemyMoves = enemy.GetMoves();
                    if (enemyMoves.Any(m => (int)m.X == (int)kingPos.X && (int)m.Y == (int)kingPos.Y))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Diese Methode gibt ein Tuple zurück, das einerseits die Anzahl legaler Züge im aktuellen Zustand 
        /// (z. B. 40 nach dem ersten Zug) und andererseits die Anzahl der verschiedenen Boardvarianten 
        /// enthält, die sich durch Simulation von MovesAhead Zügen ergeben (z. B. 400 nach dem ersten Zug).
        /// </summary>
        public (int legalMoves, long boardVariants) CountMovesAndVariants(_4ChessGame game)
        {
            int legalMoves = CountTotalLegalMoves(game);
            long boardVariants = CountBoardVariants(game, MovesAhead);
            return (legalMoves, boardVariants);
        }
    }
}
