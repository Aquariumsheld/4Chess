using _4Chess.Game;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace _4Chess.Pieces
{
    public abstract class Piece
    {
        public _4ChessGame? Game { get; set; }

        public string? FilePath { get; set; }
        public int X { get; set; }

        public int Y { get; set; }

        public Color Alignment { get; set; }

        public enum Color
        {
            Black,
            White,
            None
        }

        public abstract List<Vector2> GetMoves(bool validate = true, bool rocharde = true);

        public List<Vector2> GetAllEnemyMoves()
        {
            List<Vector2> moves = [];

            if (Game != null)
            {
                List<Piece> temp = [.. Game.Board.SelectMany(x => x).Where(elem => elem != null && elem.Alignment != this.Alignment)];

                foreach (var piece in temp)
                {
                    moves.AddRange(piece.GetMoves(false, false));
                }
            }
            
            return moves;
        }

        protected bool AddMoveIfValid(List<Vector2> moves, int x, int y)
        {
            if (x >= 0 && x < Game?.Board.Count && y >= 0 && y < Game.Board.Count)
            {
                var piece = Game.Board[y][x];

                if (piece == null)
                {
                    moves.Add(new Vector2(x, y));
                    return true;
                }

                else if(piece.Alignment != this.Alignment)
                {
                    moves.Add(new Vector2(x, y));
                    return false;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public List<Vector2> ValidateMoves(List<Vector2> moves)
        {
            if (Game != null)
            {
                for (int i = moves.Count - 1; i >= 0; i--)
                {
                    //aktuellen Zustand speichern
                    int tempY = (int)moves[i].Y;
                    int tempX = (int)moves[i].X;

                    Piece? tileContent = Game.Board[tempY][tempX];
                    Game.Board[tempY][tempX] = this;
                    Game.Board[Y][X] = null;

                    //alle jetzt gegnerischen Züge ohne Validierung abfragen
                    List<Vector2> enemyMoves = 
                        [.. Game.Board.
                        SelectMany(x => x).
                        Where(elem => elem != null && 
                        elem.Alignment != Alignment).
                        SelectMany(p => p?.GetMoves(false) ?? [])];

                    if(typeof(King) == GetType())
                    {
                        if (enemyMoves.Contains(moves[i])) moves.RemoveAt(i);
                    }
                    else
                    {
                        //prüfen, ob König bedroht würde
                        if (enemyMoves.Contains(GetKingPosition() ?? new Vector2(-99f, -99f))) moves.RemoveAt(i);
                    }

                    //vorherigen Zustand wiederherstellen
                    Game.Board[tempY][tempX] = tileContent;
                    Game.Board[Y][X] = this;
                }

                return moves;
            }

            return [];
        }

        public Vector2? GetKingPosition()
        {
            if(Game != null)
            {
                Vector2 kingPosition = Alignment switch
                {
                    Color.Black => Game.BlackKingPosition,
                    Color.White => Game.WhiteKingPosition,
                    //Kann eigentlich nicht passieren
                    _ => new()
                };

                return kingPosition;
            }

            return null;
        }
    }
}
