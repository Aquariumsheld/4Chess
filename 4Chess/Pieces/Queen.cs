using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using _4Chess;

namespace _4Chess.Pieces
{
    class Queen : Piece
    {
        public Queen(int yPosition, int xPosition, Color alignment)
        {
            Y = yPosition;
            X = xPosition;
            FilePath = alignment == Color.White ? "WhiteQueen.png" : "BlackQueen.png";
            Alignment = alignment;

            PossibleMoves = GetMoves();
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

            for (int i = 1; i < TempGame.Board.Count; i++)
            {
                //Felder links der Figur
                if (X - i >= 0 && left)
                {
                    if (TempGame.Board[Y][X - i] == null)
                        moves.Add(new Vector2(X - i, Y));

                    else if (TempGame.Board[Y][X - i]?.Alignment != this.Alignment)
                    {
                        moves.Add(new Vector2(X - i, Y));
                        left = false;
                    }

                    else left = false;
                }

                //Felder rechts der Figur
                if (X + i <= 7 && right)
                {
                    if (TempGame.Board[Y][X + i] == null)
                        moves.Add(new Vector2(X + i, Y));

                    else if (TempGame.Board[Y][X + i]?.Alignment != this.Alignment)
                    {
                        moves.Add(new Vector2(X + i, Y));
                        right = false;
                    }

                    else right = false;
                }

                //Felder oberhalb der Figur
                if (Y - i >= 0 && up)
                {
                    if (TempGame.Board[Y - i][X] == null)
                        moves.Add(new Vector2(X, Y - i));

                    else if (TempGame.Board[Y - i][X]?.Alignment != this.Alignment)
                    {
                        moves.Add(new Vector2(X, Y - i));
                        up = false;
                    }

                    else up = false;
                }

                //Felder oberhalb der Figur
                if (Y + i <= 7 && down)
                {
                    if (TempGame.Board[Y + i][X] == null)
                        moves.Add(new Vector2(X, Y + i));

                    else if (TempGame.Board[Y + i][X]?.Alignment != this.Alignment)
                    {
                        moves.Add(new Vector2(X, Y + i));
                        down = false;
                    }
                    else down = false;
                }

                //Felder links über der Figur
                if (X - i >= 0 && Y - i >= 0 && leftUp)
                {
                    if (TempGame.Board[Y - i][X - i] == null)
                        moves.Add(new Vector2(X - i, Y - i));

                    else if (TempGame.Board[Y - i][X - i]?.Alignment != this.Alignment)
                    {
                        moves.Add(new Vector2(X - i, Y - i));
                        leftUp = false;
                    }

                    else leftUp = false;
                }

                //Felder rechts über der Figur
                if (X + i < TempGame.Board.Count && Y - i < TempGame.Board.Count && rightUp)
                {
                    if (TempGame.Board[Y - i][X + i] == null)
                        moves.Add(new Vector2(X + i, Y - i));

                    else if (TempGame.Board[Y][X + i]?.Alignment != this.Alignment)
                    {
                        moves.Add(new Vector2(X + i, Y - i));
                        rightUp = false;
                    }

                    else rightUp = false;
                }

                //Felder links unter der Figur
                if (Y + i >= 0 && X - i >= 0 && leftDown)
                {
                    if (TempGame.Board[Y + i][X - i] == null)
                        moves.Add(new Vector2(X - i, Y + i));

                    else if (TempGame.Board[Y + i][X - i]?.Alignment != this.Alignment)
                    {
                        moves.Add(new Vector2(X - i, Y + i));
                        leftDown = false;
                    }

                    else leftDown = false;
                }

                //Felder rechts unter der Figur
                if (Y + i < TempGame.Board.Count && X + i < TempGame.Board.Count && rightDown)
                {
                    if (TempGame.Board[Y + i][X + i] == null)
                        moves.Add(new Vector2(X + i, Y + i));

                    else if (TempGame.Board[Y + i][X + i]?.Alignment != this.Alignment)
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
