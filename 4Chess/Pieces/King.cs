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
        public bool IsUnmoved { get; set; } = true;

        public King(int yPosition, int xPosition, Color alignment, _4ChessGame game)
        {
            Y = yPosition;
            X = xPosition;
            Alignment = alignment;
            FilePath = alignment == Color.White ? "WhiteKing.png" : "BlackKing.png";
            Game = game;

            PossibleMoves = GetMoves();
        }

        public override List<Vector2> GetMoves()
        {
            List<Vector2> moves = [];

            //Felder links der Figur
            if (X - 1 >= 0)
            {
                if (Game.Board[Y][X - 1] == null)
                    moves.Add(new Vector2(X-1,Y));

                else if (Game.Board[Y][X - 1]?.Alignment != this.Alignment)
                {
                    moves.Add(new Vector2(X - 1, Y));
                }
            }

            //Felder rechts der Figur
            if (X + 1 <= 7)
            {
                if (Game.Board[Y][X + 1] == null)
                    moves.Add(new Vector2(X + 1, Y));

                else if (Game.Board[Y][X + 1]?.Alignment != this.Alignment)
                {
                    moves.Add(new Vector2(X + 1, Y));
                }

            }

            //Felder oberhalb der Figur
            if (Y - 1 >= 0)
            {
                if (Game.Board[Y - 1][X] == null)
                    moves.Add(new Vector2(X, Y - 1));

                else if (Game.Board[Y - 1][X]?.Alignment != this.Alignment)
                {
                    moves.Add(new Vector2(X, Y - 1));
                }

            }

            //Felder oberhalb der Figur
            if (Y + 1 <= 7)
            {
                if (Game.Board[Y + 1][X] == null)
                    moves.Add(new Vector2(X, Y + 1));

                else if (Game.Board[Y + 1][X]?.Alignment != this.Alignment)
                {
                    moves.Add(new Vector2(X, Y + 1));
                }
            }

            //Felder links über der Figur
            if (X - 1 >= 0 && Y - 1 >= 0)
            {
                if (Game.Board[Y - 1][X - 1] == null)
                    moves.Add(new Vector2(X - 1, Y - 1));

                else if (Game.Board[Y - 1][X - 1]?.Alignment != this.Alignment)
                {
                    moves.Add(new Vector2(X - 1, Y - 1));
                }

            }

            //Felder rechts über der Figur
            if (X + 1 < Game.Board.Count && Y - 1 < Game.Board.Count)
            {
                if (Game.Board[Y - 1][X + 1] == null)
                    moves.Add(new Vector2(X + 1, Y - 1));

                else if (Game.Board[Y][X + 1]?.Alignment != this.Alignment)
                {
                    moves.Add(new Vector2(X + 1, Y - 1));
                }

            }

            //Felder links unter der Figur
            if (Y + 1 >= 0 && X - 1 >= 0)
            {
                if (Game.Board[Y + 1][X - 1] == null)
                    moves.Add(new Vector2(X - 1, Y + 1));

                else if (Game.Board[Y + 1][X - 1]?.Alignment != this.Alignment)
                {
                    moves.Add(new Vector2(X - 1, Y + 1));
                }

            }

            //Felder rechts unter der Figur
            if (Y + 1 < Game.Board.Count && X + 1 < Game.Board.Count)
            {
                if (Game.Board[Y + 1][X + 1] == null)
                    moves.Add(new Vector2(X + 1, Y + 1));

                else if (Game.Board[Y + 1][X + 1]?.Alignment != this.Alignment)
                {
                    moves.Add(new Vector2(X + 1, Y + 1));
                }
            }

            ValidateMoves();

            return moves;
        }
    }
}
