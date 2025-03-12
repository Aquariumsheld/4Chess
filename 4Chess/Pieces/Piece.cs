using System.Numerics;

namespace _4Chess.Pieces
{
    abstract class Piece
    {
        public string FilePath { get; set; } = "";

        public static (int,int) Indexer { get; set; }

        public int X { get; set; }

        public int Y { get; set; }

        public List<Vector2> PossibleMoves { get; set; } = [];// erst Y, dann X -> Konsistenz in Reihenfolge

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
            if(typeof(King) != this.GetType())
            {
                Vector2 kingPosition = Alignment switch
                {
                    Color.Black => TempGame.BlackKingPosition,
                    Color.White => TempGame.WhiteKingPosition,
                    //Kann eigentlich nicht passieren
                    _ => new()
                };

                for (int i = 0; i < PossibleMoves.Count; i++)
                {
                    List<Piece> temp = [.. TempGame.AllPieces.Where(elem => elem.Alignment != this.Alignment)];

                    foreach (var piece in temp)
                    {
                        if (piece.PossibleMoves.Contains(kingPosition))
                            this.PossibleMoves.RemoveAt(i);
                    }
                }
            }
            else
            {
                for (int i = 0; i < PossibleMoves.Count; i++)
                {
                    List<Piece> temp = [.. TempGame.AllPieces.Where(elem => elem.Alignment != this.Alignment)];

                    foreach (var piece in temp)
                    {
                        if (piece.PossibleMoves.Contains(PossibleMoves[i]))
                            this.PossibleMoves.RemoveAt(i);
                    }
                }
            }
        }
    }
}
