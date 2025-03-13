using _4Chess.Game;
using System.Numerics;

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

        public abstract List<Vector2> GetMoves(bool validate = true);

        public List<Vector2> ValidateMoves(List<Vector2> moves)
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

                List<Piece> temp = [.. Game.Board.SelectMany(x => x).Where(elem => elem != null && elem.Alignment != this.Alignment)];

                for (int i = moves.Count - 1; i >= 0; i--)
                {
                    foreach (var piece in temp)
                    {
                        List<Vector2> enemyMoves = piece.GetMoves(false);

                        if (typeof(King) != GetType())
                        {
                            if (enemyMoves.Contains(kingPosition) || enemyMoves.Contains(new Vector2(X,Y)))
                            {
                                Piece? tileContent = Game.Board[(int)moves[i].Y][(int)moves[i].X];
                                Game.Board[(int)moves[i].Y][(int)moves[i].X] = this;
                                Game.Board[Y][X] = null;

                                if (piece.GetMoves(false).Contains(kingPosition))
                                {
                                    Game.Board[(int)moves[i].Y][(int)moves[i].X] = tileContent;
                                    Game.Board[Y][X] = this;
                                    moves.RemoveAt(i);
                                }

                                else
                                {
                                    Game.Board[Y][X] = this;
                                    Game.Board[(int)moves[i].Y][(int)moves[i].X] = tileContent;
                                }   
                            }
                        }
                        else
                        {
                            if (enemyMoves.Contains(moves[i]))
                                moves.RemoveAt(i);
                        }
                    }
                }

                return moves;
            }

            return [];
        }
    }
}
