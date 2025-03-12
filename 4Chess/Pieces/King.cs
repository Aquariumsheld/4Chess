using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _4Chess;

namespace _4Chess.Pieces
{
    class King : Piece
    {
        public bool Unmoved { get; set; } = true;

        public King(int yPosition, int xPosition, Color alignment)
        {
            Y = yPosition;
            X = xPosition;
            Alignment = alignment;
            FilePath = alignment == Color.White ? "Assets/WhiteKing.png" : "Assets/BlackKing.png";
            PossibleMoves = GetMoves();
        }

        public override List<(int, int)> GetMoves()
        {
            List<(int, int)> moves = [];

            int TravelDistance = 1;
            //Felder links der Figur
            if (X - TravelDistance >= 0)
            {
                if (TempGame.Board[Y][X - TravelDistance] == null)
                    moves.Add((Y, X - TravelDistance));

                else if (TempGame.Board[Y][X - TravelDistance]?.Alignment != this.Alignment)
                {
                    moves.Add((Y, X - TravelDistance));
                }
            }

            //Felder rechts der Figur
            if (X + TravelDistance <= 7)
            {
                if (TempGame.Board[Y][X + TravelDistance] == null)
                    moves.Add((Y, X + TravelDistance));

                else if (TempGame.Board[Y][X + TravelDistance]?.Alignment != this.Alignment)
                {
                    moves.Add((Y, X + TravelDistance));
                }

            }

            //Felder oberhalb der Figur
            if (Y - TravelDistance >= 0)
            {
                if (TempGame.Board[Y - TravelDistance][X] == null)
                    moves.Add((Y - TravelDistance, X));

                else if (TempGame.Board[Y - TravelDistance][X]?.Alignment != this.Alignment)
                {
                    moves.Add((Y - TravelDistance, X));
                }

            }

            //Felder oberhalb der Figur
            if (Y + TravelDistance <= 7)
            {
                if (TempGame.Board[Y + TravelDistance][X] == null)
                    moves.Add((Y + TravelDistance, X));

                else if (TempGame.Board[Y + TravelDistance][X]?.Alignment != this.Alignment)
                {
                    moves.Add((Y + TravelDistance, X));
                }
            }

            //Felder links über der Figur
            if (X - TravelDistance >= 0 && Y - TravelDistance >= 0)
            {
                if (TempGame.Board[Y - TravelDistance][X - TravelDistance] == null)
                    moves.Add((Y - TravelDistance, X - TravelDistance));

                else if (TempGame.Board[Y - TravelDistance][X - TravelDistance]?.Alignment != this.Alignment)
                {
                    moves.Add((Y - TravelDistance, X - TravelDistance));
                }

            }

            //Felder rechts über der Figur
            if (X + TravelDistance < TempGame.Board.Count && Y - TravelDistance < TempGame.Board.Count)
            {
                if (TempGame.Board[Y - TravelDistance][X + TravelDistance] == null)
                    moves.Add((Y - TravelDistance, X + TravelDistance));

                else if (TempGame.Board[Y][X + TravelDistance]?.Alignment != this.Alignment)
                {
                    moves.Add((Y - TravelDistance, X + TravelDistance));
                }

            }

            //Felder links unter der Figur
            if (Y + TravelDistance >= 0 && X - TravelDistance >= 0)
            {
                if (TempGame.Board[Y + TravelDistance][X - TravelDistance] == null)
                    moves.Add((Y + TravelDistance, X - TravelDistance));

                else if (TempGame.Board[Y + TravelDistance][X - TravelDistance]?.Alignment != this.Alignment)
                {
                    moves.Add((Y + TravelDistance, X - TravelDistance));
                }

            }

            //Felder rechts unter der Figur
            if (Y + TravelDistance < TempGame.Board.Count && X + TravelDistance < TempGame.Board.Count)
            {
                if (TempGame.Board[Y + TravelDistance][X + TravelDistance] == null)
                    moves.Add((Y + TravelDistance, X + TravelDistance));

                else if (TempGame.Board[Y + TravelDistance][X + TravelDistance]?.Alignment != this.Alignment)
                {
                    moves.Add((Y + TravelDistance, X + TravelDistance));
                }
            }
            return moves;
        }
    }
}
