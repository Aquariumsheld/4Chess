using _4Chess.Game;
using System.Numerics;

namespace _4Chess.Pieces
{
    class Queen : Piece
    {
        public Queen(int yPosition, int xPosition, Color alignment, _4ChessGame game)
        {
            Y = yPosition;
            X = xPosition;
            FilePath = alignment == Color.White ? "WhiteQueen.png" : "BlackQueen.png";
            Alignment = alignment;
            Game = game;
        }

        public override List<Vector2> GetMoves()
        {
            List<Vector2> moves = [];

            bool left = true;
            bool right = true;
            bool up = true;
            bool down = true;
            bool leftUp = true;
            bool rightUp = true;
            bool leftDown = true;
            bool rightDown = true;

            for (int i = 1; i < Game?.Board.Count; i++)
            {
                //Felder links der Figur
                if (X - i >= 0 && left)
                {
                    if (Game.Board[Y][X - i] == null)
                        moves.Add(new Vector2(X - i, Y));

                    else if (Game.Board[Y][X - i]?.Alignment != this.Alignment)
                    {
                        moves.Add(new Vector2(X - i, Y));
                        left = false;
                    }

                    else left = false;
                }

                //Felder rechts der Figur
                if (X + i <= 7 && right)
                {
                    if (Game.Board[Y][X + i] == null)
                        moves.Add(new Vector2(X + i, Y));

                    else if (Game.Board[Y][X + i]?.Alignment != this.Alignment)
                    {
                        moves.Add(new Vector2(X + i, Y));
                        right = false;
                    }

                    else right = false;
                }

                //Felder oberhalb der Figur
                if (Y - i >= 0 && up)
                {
                    if (Game.Board[Y - i][X] == null)
                        moves.Add(new Vector2(X, Y - i));

                    else if (Game.Board[Y - i][X]?.Alignment != this.Alignment)
                    {
                        moves.Add(new Vector2(X, Y - i));
                        up = false;
                    }

                    else up = false;
                }

                //Felder oberhalb der Figur
                if (Y + i <= 7 && down)
                {
                    if (Game.Board[Y + i][X] == null)
                        moves.Add(new Vector2(X, Y + i));

                    else if (Game.Board[Y + i][X]?.Alignment != this.Alignment)
                    {
                        moves.Add(new Vector2(X, Y + i));
                        down = false;
                    }
                    else down = false;
                }

                //Felder links über der Figur
                if (X - i >= 0 && Y - i >= 0 && leftUp)
                {
                    if (Game.Board[Y - i][X - i] == null)
                        moves.Add(new Vector2(X - i, Y - i));

                    else if (Game.Board[Y - i][X - i]?.Alignment != this.Alignment)
                    {
                        moves.Add(new Vector2(X - i, Y - i));
                        leftUp = false;
                    }

                    else leftUp = false;
                }

                //Felder rechts über der Figur
                if (X + i < Game.Board.Count && Y - i >= 0 && rightUp)
                {
                    if (Game.Board[Y - i][X + i] == null)
                        moves.Add(new Vector2(X + i, Y - i));

                    else if (Game.Board[Y][X + i]?.Alignment != this.Alignment)
                    {
                        moves.Add(new Vector2(X + i, Y - i));
                        rightUp = false;
                    }

                    else rightUp = false;
                }

                //Felder links unter der Figur
                if (Y + i < Game?.Board.Count && X - i >= 0 && leftDown)
                {
                    if (Game.Board[Y + i][X - i] == null)
                        moves.Add(new Vector2(X - i, Y + i));

                    else if (Game.Board[Y + i][X - i]?.Alignment != this.Alignment)
                    {
                        moves.Add(new Vector2(X - i, Y + i));
                        leftDown = false;
                    }

                    else leftDown = false;
                }

                //Felder rechts unter der Figur
                if (Y + i < Game.Board.Count && X + i < Game.Board.Count && rightDown)
                {
                    if (Game.Board[Y + i][X + i] == null)
                        moves.Add(new Vector2(X + i, Y + i));

                    else if (Game.Board[Y + i][X + i]?.Alignment != this.Alignment)
                    {
                        moves.Add(new Vector2(X + i, Y + i));
                        rightDown = false;
                    }
                    else rightDown = false;
                }
            }

            ValidateMoves();

            return moves;
        }
    }
}
