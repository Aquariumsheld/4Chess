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
        }

        public override List<Vector2> GetMoves()
        {
            List<Vector2> moves = new List<Vector2>();

            int yDiff = Alignment switch
            {
                Color.White => -1,
                Color.Black => +1,
                _ => 0
            };

            if (yDiff == 0)
                Console.WriteLine("Der Bauer hat keine Farbe !!!");

            // Ein Feld vorwärts, falls frei
            if (Y + yDiff >= 0 && Y + yDiff < _4ChessGame.BOARD_DIMENSIONS)
            {
                if (Game.Board[Y + yDiff][X] == null)
                {
                    moves.Add(new Vector2(X, Y + yDiff));
                }
            }

            bool onStartingRow = (Alignment == Color.White && Y == 6) || (Alignment == Color.Black && Y == 1);
            if (IsUnmoved && onStartingRow && Y + yDiff * 2 >= 0 && Y + yDiff * 2 < _4ChessGame.BOARD_DIMENSIONS)
            {
                if (Game.Board[Y + yDiff][X] == null && Game.Board[Y + yDiff * 2][X] == null)
                {
                    moves.Add(new Vector2(X, Y + yDiff * 2));
                }
            }

            if (X - 1 >= 0 && Y + yDiff >= 0 && Y + yDiff < _4ChessGame.BOARD_DIMENSIONS)
            {
                var leftCapture = Game.Board[Y + yDiff][X - 1];
                if (leftCapture != null && leftCapture.Alignment != this.Alignment)
                {
                    moves.Add(new Vector2(X - 1, Y + yDiff));
                }
            }

            if (X + 1 < _4ChessGame.BOARD_DIMENSIONS && Y + yDiff >= 0 && Y + yDiff < _4ChessGame.BOARD_DIMENSIONS)
            {
                var rightCapture = Game.Board[Y + yDiff][X + 1];
                if (rightCapture != null && rightCapture.Alignment != this.Alignment)
                {
                    moves.Add(new Vector2(X + 1, Y + yDiff));
                }
            }

            ValidateMoves();
            return moves;
        }
    }
}
