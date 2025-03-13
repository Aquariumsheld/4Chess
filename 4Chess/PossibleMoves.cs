using _4Chess.Pieces;
using System.Linq;
using System.Numerics;

namespace _4Chess.Game
{
    public class MoveCounter
    {
        public static int MaxDepth = 1; // Gibt an wie viele Züge im Voraus simuliert werden sollen

        /// <summary>
        /// Zählt alle legalen Züge beider Spieler und berechnet,
        /// wie viele verschiedene Schachbrettstellungen nach diesen Zügen möglich sind.
        /// </summary>
        public (int totalMoves, int uniquePositions) CountLegalMovesAndPositions(_4ChessGame game, int depth = 0)
        {
            int totalLegalMoves = 0;
            int uniquePositions = 0;

            foreach (var row in game.Board)
            {
                for (int i = 0; i < row.Count; i++)
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

                            // Simuliere den Zug
                            SimulateMove(game, piece, move, out Piece capturedPiece);

                            // Falls wir nicht das maximale Simulationslevel erreicht haben weiter in die Tiefe gehen
                            if (depth < MaxDepth)
                            {
                                var (_, subPositions) = CountLegalMovesAndPositions(game, depth + 1);
                                uniquePositions += subPositions;
                            }
                            else
                            {
                                uniquePositions++; // Wenn wir keine weiteren Züge simulieren zählt das aktuelle Board als neue Stellung
                            }

                            // Setze das Board wieder zurück
                            UndoMove(game, piece, move, capturedPiece);
                        }
                    }
                }
            }

            return (totalLegalMoves, uniquePositions);
        }

        /// <summary>
        /// Prüft ob ein Zug legal ist indem er simuliert wird und getestet wird
        /// ob der eigene König danach im Schach steht.
        /// </summary>
        private bool IsLegalMove(_4ChessGame game, Piece piece, Vector2 move)
        {
            SimulateMove(game, piece, move, out Piece capturedPiece);
            bool legal = !IsKingInCheck(game, piece.Alignment);
            UndoMove(game, piece, move, capturedPiece);
            return legal;
        }

        /// <summary>
        /// Simuliert einen Zug indem er ausgeführt wird und das alte Feld temporär gespeichert wird.
        /// </summary>
        private void SimulateMove(_4ChessGame game, Piece piece, Vector2 move, out Piece capturedPiece)
        {
            capturedPiece = game.Board[(int)move.Y][(int)move.X];

            game.Board[piece.Y][piece.X] = null;
            game.Board[(int)move.Y][(int)move.X] = piece;
            piece.X = (int)move.X;
            piece.Y = (int)move.Y;

            if (piece is King)
            {
                if (piece.Alignment == Piece.Color.White)
                    game.WhiteKingPosition = move;
                else
                    game.BlackKingPosition = move;
            }
        }

        /// <summary>
        /// Macht eine vorher simulierte Bewegung rückgängig.
        /// </summary>
        private void UndoMove(_4ChessGame game, Piece piece, Vector2 move, Piece capturedPiece)
        {
            game.Board[(int)move.Y][(int)move.X] = capturedPiece;
            game.Board[piece.Y][piece.X] = piece;
            piece.X = (int)move.X;
            piece.Y = (int)move.Y;

            if (piece is King)
            {
                if (piece.Alignment == Piece.Color.White)
                    game.WhiteKingPosition = new Vector2(piece.X, piece.Y);
                else
                    game.BlackKingPosition = new Vector2(piece.X, piece.Y);
            }
        }

        /// <summary>
        /// Prüft, ob der König der angegebenen Farbe im Schach steht.
        /// </summary>
        private bool IsKingInCheck(_4ChessGame game, Piece.Color kingColor)
        {
            Vector2 kingPos = kingColor == Piece.Color.White ? game.WhiteKingPosition : game.BlackKingPosition;

            foreach (var row in game.Board)
            {
                for (int i = 0; i < row.Count; i++)
                {
                    var enemy = row[i];
                    if (enemy == null || enemy.Alignment == kingColor)
                        continue;

                    var enemyMoves = enemy.GetMoves();
                    if (enemyMoves.Any(m => (int)m.X == (int)kingPos.X && (int)m.Y == (int)kingPos.Y))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
