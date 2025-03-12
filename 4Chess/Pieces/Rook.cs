using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _4Chess;

namespace _4Chess.Pieces
{
    class Rook : Piece
    {
        public bool IsUnmoved { get; set; } = true;

        public Rook(int yPosition, int xPosition, Color alignment)
        {
            Y = yPosition;
            X = xPosition;
            Alignment = alignment;
            FilePath = alignment == Color.White ? "WhiteRook.png" : "BlackRook.png";

            PossibleMoves = GetMoves();
        }

        public override List<(int, int)> GetMoves()
        {
            List<(int, int)> moves = [];

            bool left = true;
            bool right = true;
            bool up = true;
            bool down = true;

            for(int i = 1; i <= TempGame.Board.Count-1; i++)
            {
                //Felder links der Figur
                if (X - i >= 0 && left)
                {
                    if (TempGame.Board[Y][X - i] == null)
                        moves.Add((Y, X - i));

                    else if (TempGame.Board[Y][X - i]?.Alignment != this.Alignment)
                    {
                        moves.Add((Y, X - i));
                        left = false;
                    }

                    else left = false;
                }

                //Felder rechts der Figur
                if(X + i <= 7 && right)
                {
                    if (TempGame.Board[Y][X + i] == null)
                        moves.Add((Y, X + i));

                    else if (TempGame.Board[Y][X + i]?.Alignment != this.Alignment)
                    {
                        moves.Add((Y, X + i));
                        right = false;
                    }

                    else right = false;
                }

                //Felder oberhalb der Figur
                if (Y - i >= 0 && up)
                {
                    if (TempGame.Board[Y - i][X] == null)
                        moves.Add((Y - i, X));

                    else if (TempGame.Board[Y - i][X]?.Alignment != this.Alignment)
                    {
                        moves.Add((Y - i, X));
                        up = false;
                    }

                    else up = false;
                }

                //Felder oberhalb der Figur
                if (Y + i <= 7 && down)
                {
                    if (TempGame.Board[Y + i][X] == null)
                        moves.Add((Y + i, X));

                    else if (TempGame.Board[Y + i][X]?.Alignment != this.Alignment)
                    {
                        moves.Add((Y + i, X));
                        down = false;
                    }
                    else down = false;
                }
            }

            ValidateMoves();

            return moves;
        }
    }
}
