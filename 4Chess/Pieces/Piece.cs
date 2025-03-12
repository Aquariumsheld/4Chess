using _4Chess.Game;
using System.Numerics;

namespace _4Chess.Pieces
{
    public abstract class Piece
    {
        public _4ChessGame? Game { get; set; }

        public string? FilePath { get; set; }

        public static (int,int) Indexer { get; set; }

        public int X { get; set; }

        public int Y { get; set; }

        public List<Vector2> PossibleMoves { get; set; } = [];

        public Color Alignment { get; set; }

        public enum Color
        {
            Black,
            White,
            None
        }

        public abstract List<Vector2> GetMoves();

        public void ValidateMoves()
        {
            Vector2 kingPosition = Alignment switch
            {
                Color.Black => Game.BlackKingPosition,
                Color.White => Game.WhiteKingPosition,
                //Kann eigentlich nicht passieren
                _ => new()
            };

            for (int i = 0; i < PossibleMoves.Count; i++)
            {
                List<Piece> temp = [.. Game.Board.SelectMany(x => x).Where(elem => elem != null && elem.Alignment != this.Alignment)];

                foreach (var piece in temp)
                {
                    if (typeof(King) != this.GetType())
                    {
                        if (piece.PossibleMoves.Contains(kingPosition))
                            this.PossibleMoves.RemoveAt(i);
                    }
                    else
                    {
                        if (piece.PossibleMoves.Contains(PossibleMoves[i]))
                            this.PossibleMoves.RemoveAt(i);
                    }
                }
            }
        }
    }
}
