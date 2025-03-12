using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _4Chess.Pieces
{
    class Pawn : Piece
    {
        public bool Unmoved { get; set; } = true;

        public Pawn(int yPosition, int xPosition, Color alignment)
        {
            Y = yPosition;
            X = xPosition;
            Alignment = alignment;

            PossibleMoves = GetMoves();
        }

        public override List<(int, int)> GetMoves()
        {
            List<(int, int)> moves = [];

            int yDiff = Alignment switch
            {
                Color.White => -1,
                Color.Black => +1,
                _ => 0
            };

            if (yDiff == 0) Console.WriteLine("Der Bauer hat keine Farbe !!!");

            moves.Add((Y + yDiff, X));

            if (Unmoved) moves.Add((Y + yDiff * 2, X));

            if(X - 1 >= 0)
            {
                if (TempGame.Board[Y + yDiff][X - 1]?.Alignment != this.Alignment)
                    moves.Add((Y + yDiff, X - 1));
            }

            if (X + 1 >= 0)
            {
                if (TempGame.Board[Y + yDiff][X + 1]?.Alignment != this.Alignment)
                    moves.Add((Y + yDiff, X + 1));
            }

            //en passant implementieren

            return moves;
        }
    }
}
