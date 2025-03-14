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

        public abstract List<Vector2> GetMoves(bool validate = true);

        internal (bool, float, float) Rocharde(King king)
        {
            bool rocharde = false;
            int rochardeLeft = 0;
            int rochardeRight = 0;
            switch (Alignment)
            {
                case Color.Black:
                    if (king.IsUnmoved && Game?.Board[0][0] is Rook rookLeft1 && rookLeft1.IsUnmoved)
                    {
                        if(Game.Board[0][1] == null && Game.Board[0][2] == null && Game.Board[0][3] == null &&
                            !GetAllEnemyMoves().Contains(new Vector2(X,Y)) &&
                            !GetAllEnemyMoves().Contains(new Vector2(0, 0)) &&
                            !GetAllEnemyMoves().Contains(new Vector2(1, 0)) &&
                            !GetAllEnemyMoves().Contains(new Vector2(2, 0)))
                        {
                            rocharde = true;
                            rochardeLeft = -1;
                        }
                    }

                    if(king.IsUnmoved && Game?.Board[0][7] is Rook rookRight1 && rookRight1.IsUnmoved)
                    {
                        if (Game.Board[0][5] == null && Game.Board[0][6] == null &&
                            !GetAllEnemyMoves().Contains(new Vector2(X, Y)) &&
                            !GetAllEnemyMoves().Contains(new Vector2(4, 0)) &&
                            !GetAllEnemyMoves().Contains(new Vector2(5, 0)) &&
                            !GetAllEnemyMoves().Contains(new Vector2(6, 0)) &&
                            !GetAllEnemyMoves().Contains(new Vector2(7, 0)))
                        {
                            rocharde = true;
                            rochardeRight = 1;
                        }
                    }
                    return (rocharde, rochardeLeft, rochardeRight);
                case Color.White:
                    if (king.IsUnmoved && Game?.Board[7][0] is Rook rookLeft2 && rookLeft2.IsUnmoved)
                    {
                        if (Game.Board[7][1] == null && Game.Board[7][2] == null && Game.Board[7][3] == null &&
                            !GetAllEnemyMoves().Contains(new Vector2(X, Y)) &&
                            !GetAllEnemyMoves().Contains(new Vector2(7, 7)) &&
                            !GetAllEnemyMoves().Contains(new Vector2(1, 7)) &&
                            !GetAllEnemyMoves().Contains(new Vector2(2, 7)))
                        {
                            rocharde = true;
                            rochardeLeft = -1;
                        }
                    }

                    if (Game?.Board[7][5] == null && Game?.Board[7][6] == null && king.IsUnmoved && Game?.Board[7][7] is Rook rookRight2 && rookRight2.IsUnmoved)
                    {
                        if (
                            !GetAllEnemyMoves().Contains(new Vector2(X, Y)) &&
                            !GetAllEnemyMoves().Contains(new Vector2(4, 7)) &&
                            !GetAllEnemyMoves().Contains(new Vector2(5, 7)) &&
                            !GetAllEnemyMoves().Contains(new Vector2(6, 7)) &&
                            !GetAllEnemyMoves().Contains(new Vector2(7, 7)))
                        {
                            rocharde = true;
                            rochardeRight = 1;
                        }
                    }
                    return (rocharde, rochardeLeft, rochardeRight);

                default:
                    break;
            }

            return (false, 0, 0);
        }

        public List<Vector2> GetAllEnemyMoves()
        {
            List<Vector2> moves = [];
            List<Piece> temp = [.. Game.Board.SelectMany(x => x).Where(elem => elem != null && elem.Alignment != this.Alignment)];
            
            if(temp != null)
            {
                foreach (var piece in temp)
                {
                    moves.AddRange(piece.GetMoves(false));
                }
            }
            return moves;
        }

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
