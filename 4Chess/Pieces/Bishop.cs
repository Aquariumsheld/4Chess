using _4Chess.Game;
using System.Numerics;

namespace _4Chess.Pieces
{
    class Bishop : Piece
    {
        public Bishop(int yPosition, int xPosition, Color alignment, _4ChessGame game)
        {
            Y = yPosition;
            X = xPosition;
            FilePath = alignment == Color.White ? "WhiteBishop.png" : "BlackBishop.png";
            Alignment = alignment;
            Game = game;
        }

        public override List<Vector2> GetMoves(bool validate = true)
        {
            List<Vector2> moves = [];

            bool leftUp = true;
            bool rightUp = true;
            bool leftDown = true;
            bool rightDown = true;

            for (int i = 1; i < Game?.Board.Count; i++)
            {
                //Felder links über der Figur
                if (X - i >= 0 && Y - i >= 0 && leftUp)
                {
                    if (Game.Board[Y - i][X - i] == null)
                        moves.Add(new Vector2(X-i,Y-i));

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

                    else if (Game.Board[Y - i][X + i]?.Alignment != this.Alignment)
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
                if (Y + i < Game?.Board.Count && X + i < Game.Board.Count && rightDown)
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

            if (validate) 
                return ValidateMoves(moves);

            else 
                return moves;
        }
    }
}
