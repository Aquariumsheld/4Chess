using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _4Chess.Pieces
{
    class Bishop : Piece
    {
        public bool Unmoved { get; set; } = true;

        public Bishop(int yPosition, int xPosition, Color alignment)
        {
            Y = yPosition;
            X = xPosition;
            FilePath = alignment == Color.White ? "Assets/WhiteBishop.png" : "Assets/BlackBishop.png";
            Alignment = alignment;

            PossibleMoves = GetMoves();
        }

        public override List<(int, int)> GetMoves()
        {
            List<(int, int)> moves = [];

            bool leftUp = true;
            bool rightUp = true;
            bool leftDown = true;
            bool rightDown = true;

            for (int i = 1; i < TempGame.Board.Count; i++)
            {
                //Felder links über der Figur
                if (X - i >= 0 && Y - i >= 0 && leftUp)
                {
                    if (TempGame.Board[Y - i][X - i] == null)
                        moves.Add((Y - i, X - i));

                    else if (TempGame.Board[Y - i][X - i]?.Alignment != this.Alignment)
                    {
                        moves.Add((Y - i, X - i));
                        leftUp = false;
                    }

                    else leftUp = false;
                }

                //Felder rechts über der Figur
                if (X + i < TempGame.Board.Count && Y - i < TempGame.Board.Count && rightUp)
                {
                    if (TempGame.Board[Y - i][X + i] == null)
                        moves.Add((Y - i, X + i));

                    else if (TempGame.Board[Y][X + i]?.Alignment != this.Alignment)
                    {
                        moves.Add((Y - i, X + i));
                        rightUp = false;
                    }

                    else rightUp = false;
                }

                //Felder links unter der Figur
                if (Y + i >= 0 && X - i >= 0 && leftDown)
                {
                    if (TempGame.Board[Y + i][X - i] == null)
                        moves.Add((Y + i, X - i));

                    else if (TempGame.Board[Y + i][X - i]?.Alignment != this.Alignment)
                    {
                        moves.Add((Y + i, X - i));
                        leftDown = false;
                    }

                    else leftDown = false;
                }

                //Felder rechts unter der Figur
                if (Y + i < TempGame.Board.Count && X + i < TempGame.Board.Count && rightDown)
                {
                    if (TempGame.Board[Y + i][X + i] == null)
                        moves.Add((Y + i, X + i));

                    else if (TempGame.Board[Y + i][X + i]?.Alignment != this.Alignment)
                    {
                        moves.Add((Y + i, X + i));
                        rightDown = false;
                    }
                    else rightDown = false;
                }
            }
            return moves;
        }
    }
}
