using _4Chess.Game;
using System.Numerics;

namespace _4Chess.Pieces
{
    class Pawn : Piece
    {
        public bool IsUnmoved { get; set; } = true;
        public bool IsEnPassant { get; set; } = false;

        public Pawn(int yPosition, int xPosition, Color alignment, _4ChessGame game)
        {
            Y = yPosition;
            X = xPosition;
            Alignment = alignment;
            FilePath = alignment == Color.White ? "WhitePawn.png" : "BlackPawn.png";
            Game = game;

            PossibleMoves = GetMoves();
        }

        public override List<Vector2> GetMoves()
        {
            List<Vector2> moves = [];

            int yDiff = Alignment switch
            {
                Color.White => -1,
                Color.Black => +1,
                _ => 0
            };

            if (yDiff == 0) Console.WriteLine("Der Bauer hat keine Farbe !!!");

            moves.Add(new Vector2(X, Y + yDiff));

            if (IsUnmoved)
            {
                moves.Add(new Vector2(X, Y + yDiff * 2));
            }

            if (X - 1 >= 0 && Y + yDiff >= 0 && Y + yDiff < Game?.Board.Count)
            {
                if (Game?.Board[Y + yDiff][X - 1]?.Alignment != this.Alignment ||
                    (Game.Board[Y][X - 1] is Pawn leftPawn && leftPawn.Alignment != this.Alignment && leftPawn.IsEnPassant))
                    moves.Add(new Vector2(X - 1, Y + yDiff));
            }

            if (X + 1 < Game?.Board.Count)
            {
                if (Game.Board[Y + yDiff][X + 1]?.Alignment != this.Alignment ||
                     (Game.Board[Y][X - 1] is Pawn rightPawn && rightPawn.Alignment != this.Alignment && rightPawn.IsEnPassant))
                    moves.Add(new Vector2(X + 1, Y + yDiff));
            }

            ValidateMoves();

            return moves;
        }
    }
}
