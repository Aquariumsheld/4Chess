using _4Chess.Game;
using _4Chess.Pieces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace _4Chess
{
    public class MoveCounter
    {
        /// <summary>
        /// Gibt an, wie viele volle Züge (jeweils Weiß und Schwarz) im Voraus simuliert werden sollen.
        /// Eine Tiefe von 1 simuliert also einen Zug (erst Weiß, dann Schwarz).
        /// </summary>
        public int MaxDepth { get; set; } = 2;

        /// <summary>
        /// Zählt die Gesamtzahl der legalen vollen Züge und ermittelt,
        /// wie viele unterschiedliche Brettstellungen nach den simulierten Zügen möglich sind.
        /// Es wird davon ausgegangen, dass im Ausgangszustand Weiß am Zug ist.
        /// </summary>
        public (int totalFullMoves, int uniquePositions) CountFullMovesAndPositions(_4ChessGame game)
        {
            var uniquePositions = new HashSet<string>();
            int totalFullMoves = CountFullMovesRecursive(game, 0, uniquePositions);
            return (totalFullMoves, uniquePositions.Count);
        }

        /// <summary>
        /// Simuliert rekursiv einen vollen Zug (Zug von Weiß gefolgt von Zug von Schwarz).
        /// Der Parameter <paramref name="depth"/> gibt an, wie viele volle Züge bereits simuliert wurden.
        /// </summary>
        private int CountFullMovesRecursive(_4ChessGame game, int depth, HashSet<string> uniquePositions)
        {
            int totalFullMoves = 0;

            // Durchlaufe das gesamte Board und suche alle weißen Figuren
            for (int y = 0; y < game.Board.Count; y++)
            {
                for (int x = 0; x < game.Board[y].Count; x++)
                {
                    var whitePiece = game.Board[y][x];
                    if (whitePiece == null || whitePiece.Alignment != Piece.Color.White)
                        continue;

                    var whiteMoves = whitePiece.GetMoves();
                    foreach (var whiteMove in whiteMoves)
                    {
                        // Prüfe zunächst, ob der weiße Zug legal ist
                        if (!IsLegalMove(game, whitePiece, whiteMove))
                            continue;

                        // Simuliere den weißen Zug
                        var whiteMoveData = SimulateMove(game, whitePiece, whiteMove);

                        // Nun: Für den neuen Zustand alle legalen Züge der schwarzen Figuren berechnen
                        for (int yBlack = 0; yBlack < game.Board.Count; yBlack++)
                        {
                            for (int xBlack = 0; xBlack < game.Board[yBlack].Count; xBlack++)
                            {
                                var blackPiece = game.Board[yBlack][xBlack];
                                if (blackPiece == null || blackPiece.Alignment != Piece.Color.Black)
                                    continue;

                                var blackMoves = blackPiece.GetMoves();
                                foreach (var blackMove in blackMoves)
                                {
                                    if (!IsLegalMove(game, blackPiece, blackMove))
                                        continue;

                                    // Simuliere den schwarzen Zug
                                    var blackMoveData = SimulateMove(game, blackPiece, blackMove);

                                    // Ein vollständiger Zug (weiß und schwarz) wurde erfolgreich simuliert.
                                    totalFullMoves++;

                                    if (depth < MaxDepth - 1)
                                    {
                                        totalFullMoves += CountFullMovesRecursive(game, depth + 1, uniquePositions);
                                    }
                                    else
                                    {
                                        string boardState = SerializeBoard(game.Board);
                                        uniquePositions.Add(boardState);
                                    }

                                    // Mache den schwarzen Zug rückgängig
                                    UndoMove(game, blackPiece, blackMove, blackMoveData);
                                }
                            }
                        }

                        // Mache den weißen Zug rückgängig
                        UndoMove(game, whitePiece, whiteMove, whiteMoveData);
                    }
                }
            }

            return totalFullMoves;
        }

        /// <summary>
        /// Prüft, ob ein gegebener Zug für eine Figur legal ist,
        /// also ob dadurch der eigene König nicht in Schach gerät.
        /// </summary>
        private static bool IsLegalMove(_4ChessGame game, Piece piece, Vector2 move)
        {
            var moveData = SimulateMove(game, piece, move);
            bool legal = !IsKingInCheck(game, piece.Alignment);
            UndoMove(game, piece, move, moveData);
            return legal;
        }

        /// <summary>
        /// Simuliert einen Zug, indem die Figur vom Ausgangsfeld entfernt und auf das Zielfeld gesetzt wird.
        /// Dabei werden die ursprünglichen Koordinaten sowie ein eventuell geschlagenes Piece zurückgegeben.
        /// </summary>
        private static (int origX, int origY, Piece? capturedPiece) SimulateMove(_4ChessGame game, Piece piece, Vector2 move)
        {
            int origX = piece.X;
            int origY = piece.Y;
            Piece? capturedPiece = game.Board[(int)move.Y][(int)move.X];

            game.Board[origY][origX] = null;
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

            return (origX, origY, capturedPiece);
        }

        /// <summary>
        /// Macht einen zuvor simulierten Zug rückgängig, indem die Figur an ihre ursprüngliche Position zurückgesetzt wird
        /// und ein eventuell geschlagenes Piece wieder an seinen Platz gesetzt wird.
        /// </summary>
        private static void UndoMove(_4ChessGame game, Piece piece, Vector2 move, (int origX, int origY, Piece? capturedPiece) moveData)
        {
            game.Board[(int)move.Y][(int)move.X] = moveData.capturedPiece;
            game.Board[moveData.origY][moveData.origX] = piece;
            piece.X = moveData.origX;
            piece.Y = moveData.origY;

            if (piece is King)
            {
                if (piece.Alignment == Piece.Color.White)
                    game.WhiteKingPosition = new Vector2(piece.X, piece.Y);
                else
                    game.BlackKingPosition = new Vector2(piece.X, piece.Y);
            }
        }

        /// <summary>
        /// Prüft, ob der König der angegebenen Farbe im aktuellen Spielzustand in Schach steht.
        /// Dazu werden die von allen gegnerischen Figuren angegriffenen Felder ermittelt.
        /// </summary>
        private static bool IsKingInCheck(_4ChessGame game, Piece.Color kingColor)
        {
            Vector2 kingPos = kingColor == Piece.Color.White ? game.WhiteKingPosition : game.BlackKingPosition;

            foreach (var row in game.Board)
            {
                for (int x = 0; x < row.Count; x++)
                {
                    var enemy = row[x];
                    if (enemy == null || enemy.Alignment == kingColor)
                        continue;

                    foreach (var square in GetAttackedSquares(enemy, game.Board))
                    {
                        if ((int)square.X == (int)kingPos.X && (int)square.Y == (int)kingPos.Y)
                            return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Ermittelt die von einer gegnerischen Figur angegriffenen Felder.
        /// Spezialbehandlungen gibt es für Pawn, Knight, Bishop, Rook, Queen und King.
        /// </summary>
        private static IEnumerable<Vector2> GetAttackedSquares(Piece enemy, List<List<Piece?>> board)
        {
            if (enemy is Pawn pawn)
            {
                int direction = pawn.Alignment == Piece.Color.White ? 1 : -1;
                int newY = pawn.Y + direction;
                if (newY >= 0 && newY < board.Count)
                {
                    if (pawn.X > 0)
                        yield return new Vector2(pawn.X - 1, newY);
                    if (pawn.X < board[newY].Count - 1)
                        yield return new Vector2(pawn.X + 1, newY);
                }
            }
            else if (enemy is Knight knight)
            {
                int[] dx = [2, 1, -1, -2, -2, -1, 1, 2];
                int[] dy = [1, 2, 2, 1, -1, -2, -2, -1];
                for (int i = 0; i < 8; i++)
                {
                    int newX = knight.X + dx[i];
                    int newY = knight.Y + dy[i];
                    if (newX >= 0 && newY >= 0 && newY < board.Count && newX < board[newY].Count)
                        yield return new Vector2(newX, newY);
                }
            }
            else if (enemy is Bishop bishop)
            {
                foreach (var dir in GetDirectionSquares(bishop, [1, 1, -1, -1], [1, -1, 1, -1], board))
                    yield return dir;
            }
            else if (enemy is Rook rook)
            {
                foreach (var dir in GetDirectionSquares(rook, [1, 0, -1, 0], [0, 1, 0, -1], board))
                    yield return dir;
            }
            else if (enemy is Queen queen)
            {
                foreach (var dir in GetDirectionSquares(queen, [1, 1, -1, -1, 1, 0, -1, 0], [1, -1, 1, -1, 0, 1, 0, -1], board))
                    yield return dir;
            }
            else if (enemy is King king)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0)
                            continue;
                        int newX = king.X + dx;
                        int newY = king.Y + dy;
                        if (newX >= 0 && newY >= 0 && newY < board.Count && newX < board[newY].Count)
                            yield return new Vector2(newX, newY);
                    }
                }
            }
        }

        /// <summary>
        /// Liefert alle Felder in den angegebenen Richtungen ausgehend von der Position der Figur.
        /// </summary>
        private static IEnumerable<Vector2> GetDirectionSquares(Piece piece, int[] dxArray, int[] dyArray, List<List<Piece?>> board)
        {
            for (int i = 0; i < dxArray.Length; i++)
            {
                int dx = dxArray[i];
                int dy = dyArray[i];
                int newX = piece.X + dx;
                int newY = piece.Y + dy;
                while (newX >= 0 && newY >= 0 && newY < board.Count && newX < board[newY].Count)
                {
                    yield return new Vector2(newX, newY);
                    if (board[newY][newX] != null)
                        break;
                    newX += dx;
                    newY += dy;
                }
            }
        }

        /// <summary>
        /// Serialisiert den aktuellen Zustand des Boards in einen String, der zur Identifikation einzigartiger Stellungen dient.
        /// </summary>
        private static string SerializeBoard(List<List<Piece?>> board)
        {
            var sb = new StringBuilder();
            foreach (var row in board)
            {
                foreach (var piece in row)
                {
                    if (piece == null)
                    {
                        sb.Append('.');
                        continue;
                    }
                    char c = piece switch
                    {
                        Pawn => 'P',
                        Knight => 'N',
                        Bishop => 'B',
                        Rook => 'R',
                        Queen => 'Q',
                        King => 'K',
                        _ => '?'
                    };
                    // Beispiel: Kleinbuchstabe für Weiß, Großbuchstabe für Schwarz
                    sb.Append(piece.Alignment == Piece.Color.White ? char.ToLower(c) : c);
                }
                sb.Append('|');
            }
            return sb.ToString();
        }
    }
}
