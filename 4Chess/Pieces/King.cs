using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using _4Chess;
using _4Chess.Game;

namespace _4Chess.Pieces
{
    class King : Piece
    {
        /// <summary>
        /// Legt fest, ob die Figur im gesamten Spielverlauf schon einmal bewegt wurde
        /// </summary>
        public bool IsUnmoved { get; set; } = true;

        public King(int yPosition, int xPosition, Color alignment, _4ChessGame game)
        {
            Y = yPosition;
            X = xPosition;
            Alignment = alignment;
            FilePath = alignment == Color.White ? "WhiteKing.png" : "BlackKing.png";
            Game = game;

            switch (Alignment)
            {
                case Color.Black:
                    game.BlackKingPosition = new Vector2(xPosition, yPosition);
                    break;
                case Color.White:
                    game.WhiteKingPosition = new Vector2(xPosition, yPosition);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Ermittelt alle für den König möglichen Züge in Abhängigkeit von verbündeten und feindlichen Spielfiguren.
        /// </summary>
        /// <param name="validate">Legt fest, ob die Methode im Rahmen der Methode ValidateMoves() aufgerufen wird. Sollte dies der Fall sein, so wird durch
        /// diesen Wert eine Rekursion vermieden.</param>
        /// <returns>Eine Liste mit allen für die Figur mögliche Züge</returns>
        public override List<Vector2> GetMoves(bool validate = true)
        {
            List<Vector2> moves = [];

            //Felder links der Figur
            if (X - 1 >= 0)
            {
                if (Game?.Board[Y][X - 1] == null)
                    moves.Add(new Vector2(X-1,Y));

                else if (Game.Board[Y][X - 1]?.Alignment != this.Alignment)
                {
                    moves.Add(new Vector2(X - 1, Y));
                }
            }

            //Felder rechts der Figur
            if (X + 1 < Game?.Board.Count)
            {
                if (Game?.Board[Y][X + 1] == null)
                    moves.Add(new Vector2(X + 1, Y));

                else if (Game.Board[Y][X + 1]?.Alignment != this.Alignment)
                {
                    moves.Add(new Vector2(X + 1, Y));
                }

            }

            //Felder oberhalb der Figur
            if (Y - 1 >= 0)
            {
                if (Game?.Board[Y - 1][X] == null)
                    moves.Add(new Vector2(X, Y - 1));

                else if (Game.Board[Y - 1][X]?.Alignment != this.Alignment)
                {
                    moves.Add(new Vector2(X, Y - 1));
                }

            }

            //Felder oberhalb der Figur
            if (Y + 1 < Game?.Board.Count)
            {
                if (Game?.Board[Y + 1][X] == null)
                    moves.Add(new Vector2(X, Y + 1));

                else if (Game.Board[Y + 1][X]?.Alignment != this.Alignment)
                {
                    moves.Add(new Vector2(X, Y + 1));
                }
            }

            //Felder links über der Figur
            if (X - 1 >= 0 && Y - 1 >= 0)
            {
                if (Game?.Board[Y - 1][X - 1] == null)
                    moves.Add(new Vector2(X - 1, Y - 1));

                else if (Game.Board[Y - 1][X - 1]?.Alignment != this.Alignment)
                {
                    moves.Add(new Vector2(X - 1, Y - 1));
                }

            }

            //Felder rechts über der Figur
            if (X + 1 < Game?.Board.Count && Y - 1 >= 0)
            {
                if (Game?.Board[Y - 1][X + 1] == null)
                    moves.Add(new Vector2(X + 1, Y - 1));

                else if (Game.Board[Y - 1][X + 1]?.Alignment != this.Alignment)
                {
                    moves.Add(new Vector2(X + 1, Y - 1));
                }

            }

            //Felder links unter der Figur
            if (Y + 1 < Game?.Board.Count && X - 1 >= 0)
            {
                if (Game?.Board[Y + 1][X - 1] == null)
                    moves.Add(new Vector2(X - 1, Y + 1));

                else if (Game.Board[Y + 1][X - 1]?.Alignment != this.Alignment)
                {
                    moves.Add(new Vector2(X - 1, Y + 1));
                }

            }

            //Felder rechts unter der Figur
            if (Y + 1 < Game?.Board.Count && X + 1 < Game.Board.Count)
            {
                if (Game?.Board[Y + 1][X + 1] == null)
                    moves.Add(new Vector2(X + 1, Y + 1));

                else if (Game.Board[Y + 1][X + 1]?.Alignment != this.Alignment)
                {
                    moves.Add(new Vector2(X + 1, Y + 1));
                }
            }

            if (validate)
                return ValidateMoves(moves);

            else
                return moves;
        }
    }
}
