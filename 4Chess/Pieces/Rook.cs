using _4Chess.Game;
using System.Numerics;

namespace _4Chess.Pieces
{
    class Rook : Piece
    {
        public bool IsUnmoved { get; set; } = true;

        public Rook(int yPosition, int xPosition, Color alignment, _4ChessGame game)
        {
            Y = yPosition;
            X = xPosition;
            Alignment = alignment;
            FilePath = alignment == Color.White ? "WhiteRook.png" : "BlackRook.png";
            Game = game;

            PossibleMoves = GetMoves();
        }

        public override List<Vector2> GetMoves()
        {
            List<Vector2> moves = [];

            bool left = true;
            bool right = true;
            bool up = true;
            bool down = true;

            for(int i = 1; i < Game?.Board.Count; i++)
            {
                //Felder links der Figur
                if (X - i >= 0 && left)
                {
                    if (Game.Board[Y][X - i] == null)
                        moves.Add(new Vector2(X - i, Y));

                    else if (Game.Board[Y][X - i]?.Alignment != this.Alignment)
                    {
                        moves.Add(new Vector2(X - i, Y));
                        left = false;
                    }

                    else left = false;
                }

                //Felder rechts der Figur
                if(X + i <= 7 && right)
                {
                    if (Game.Board[Y][X + i] == null)
                        moves.Add(new Vector2(X + i, Y));

                    else if (Game.Board[Y][X + i]?.Alignment != this.Alignment)
                    {
                        moves.Add(new Vector2(X + i, Y));
                        right = false;
                    }

                    else right = false;
                }

                //Felder oberhalb der Figur
                if (Y - i >= 0 && up)
                {
                    if (Game.Board[Y - i][X] == null)
                        moves.Add(new Vector2(X, Y - i));

                    else if (Game.Board[Y - i][X]?.Alignment != this.Alignment)
                    {
                        moves.Add(new Vector2(X, Y - i));
                        up = false;
                    }

                    else up = false;
                }

                //Felder oberhalb der Figur
                if (Y + i <= 7 && down)
                {
                    if (Game.Board[Y + i][X] == null)
                        moves.Add(new Vector2(X, Y + i));

                    else if (Game.Board[Y + i][X]?.Alignment != this.Alignment)
                    {
                        moves.Add(new Vector2(X, Y + i));
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
