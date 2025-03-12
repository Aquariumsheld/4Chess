using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _4Chess.Pieces
{
    class Knight : Piece
    {
        public Knight(int yPosition, int xPosition, Color alignment)
        {
            Y = yPosition;
            X = xPosition;
            Alignment = alignment;

            PossibleMoves = GetMoves();
        }

        public override List<(int, int)> GetMoves()
        {
            List<(int, int)> moves = [];

            //oben links
            if(X-1 >= 0 && Y-2 >= 0)
            {
                if (TempGame.Board[Y - 2][X - 1] == null || TempGame.Board[Y - 2][X - 1]?.Alignment != this.Alignment)
                    moves.Add((Y - 2, X - 1));
            }

            //oben rechts
            if (X + 1 < TempGame.Board.Count && Y - 2 >= 0)
            {
                if (TempGame.Board[Y - 2][X + 1] == null || TempGame.Board[Y - 2][X + 1]?.Alignment != this.Alignment)
                    moves.Add((Y - 2, X + 1));
            }

            //rechts oben
            if (X + 2 < TempGame.Board.Count && Y - 1 >= 0)
            {
                if (TempGame.Board[Y - 1][X + 2] == null || TempGame.Board[Y - 1][X + 2]?.Alignment != this.Alignment)
                    moves.Add((Y - 1, X + 2));
            }

            //rechts unten
            if (X + 2 < TempGame.Board.Count && Y + 1 < TempGame.Board.Count)
            {
                if (TempGame.Board[Y + 1][X + 2] == null || TempGame.Board[Y + 1][X + 2]?.Alignment != this.Alignment)
                    moves.Add((Y + 1, X + 2));
            }

            //unten rechts
            if (X + 1 < TempGame.Board.Count && Y + 2 < TempGame.Board.Count)
            {
                if (TempGame.Board[Y + 2][X + 1] == null || TempGame.Board[Y + 2][X + 1]?.Alignment != this.Alignment)
                    moves.Add((Y + 2, X + 1));
            }

            //unten links
            if (X - 1 >= 0 && Y + 2 < TempGame.Board.Count)
            {
                if (TempGame.Board[Y + 2][X - 1] == null || TempGame.Board[Y + 2][X - 1]?.Alignment != this.Alignment)
                    moves.Add((Y + 2, X - 1));
            }

            //links unten
            if (X - 2 >= 0 && Y + 1 < TempGame.Board.Count)
            {
                if (TempGame.Board[Y + 1][X - 2] == null || TempGame.Board[Y + 1][X - 2]?.Alignment != this.Alignment)
                    moves.Add((Y + 1, X - 2));
            }

            //links oben
            if (X - 2 >= 0 && Y - 1 >= 0)
            {
                if (TempGame.Board[Y - 1][X - 2] == null || TempGame.Board[Y - 1][X - 2]?.Alignment != this.Alignment)
                    moves.Add((Y - 1, X - 2));
            }

            return moves;
        }
    }
}
