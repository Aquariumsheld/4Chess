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
            if (Game != null)
            {
                Vector2 kingPosition = GetKingPosition(Game);

                List<Piece> temp = [.. Game.Board.SelectMany(x => x).Where(elem => elem != null && elem.Alignment != this.Alignment)];

                foreach (var piece in temp)
                {
                    for (int i = moves.Count - 1; i >= 0; i--)
                    {
                        List<Vector2> enemyMoves = piece.GetMoves(false);

                        if (typeof(King) != GetType())
                        {
                            if (enemyMoves.Contains(kingPosition) || enemyMoves.Contains(new Vector2(X, Y)))
                            {
                                Piece? tileContent = Game.Board[(int)moves[i].Y][(int)moves[i].X];
                                Game.Board[(int)moves[i].Y][(int)moves[i].X] = this;
                                Game.Board[Y][X] = null;

                                if ((piece.GetMoves(false).Contains(kingPosition)) && (piece.X != moves[i].X || piece.Y != moves[i].Y))
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
                            if (piece.GetType() != typeof(Pawn))
                            {
                                Piece? tileContent = Game.Board[(int)moves[i].Y][(int)moves[i].X];
                                Game.Board[(int)moves[i].Y][(int)moves[i].X] = this;
                                Game.Board[Y][X] = null;

                                if ((piece.GetMoves(false).Contains(moves[i])) && (piece.X != moves[i].X || piece.Y != moves[i].Y))
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

                            else
                            {
                                switch (Alignment)
                                {
                                    case Color.Black:
                                        moves.Remove(new Vector2(piece.X - 1, piece.Y - 1));
                                        moves.Remove(new Vector2(piece.X + 1, piece.Y - 1));
                                        break;
                                    case Color.White:
                                        moves.Remove(new Vector2(piece.X - 1, piece.Y + 1));
                                        moves.Remove(new Vector2(piece.X + 1, piece.Y + 1));
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                }

                return moves;
            }

            return [];
        }

        public Vector2 GetKingPosition(_4ChessGame game)
        {
            Vector2 kingPosition = Alignment switch
            {
                Color.Black => game.BlackKingPosition,
                Color.White => game.WhiteKingPosition,
                //Kann eigentlich nicht passieren
                _ => new()
            };

            return kingPosition;
        }
    }
}
