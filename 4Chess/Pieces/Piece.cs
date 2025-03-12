using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _4Chess.Pieces
{
    public abstract class Piece
    {
        public string FilePath { get; set; } = "";

        public static (int,int) Indexer { get; set; }
        public string FilePath { get; set; }

        public int X { get; set; }

        public int Y { get; set; }

        public List<(int, int)> PossibleMoves { get; set; } = [];// erst Y, dann X -> Konsistenz in Reihenfolge

        public List<(int, int)>? SpecialMoves { get; set; } = null;

        public Color Alignment { get; set; }

        public enum Color
        {
            Black,
            White,
            None
        }

        public abstract List<(int, int)> GetMoves();

        public void ValidateMoves()
        {
            (int, int) kingPosition = Alignment switch
            {
                Color.Black => TempGame.BlackKingPosition,
                Color.White => TempGame.WhiteKingPosition,
                //Kann eigentlich nicht passieren
                _ => (0,0)
            };

            for(int i = 0; i < PossibleMoves.Count; i++)
            {
                List<Piece> temp = [.. TempGame.AllPieces.Where(elem => elem.Alignment != this.Alignment)];

                foreach(var piece in temp)
                {
                    if (piece.PossibleMoves.Contains(kingPosition))
                        this.PossibleMoves.RemoveAt(i);
                }
            }

            //TODO Für den König eigenes Aufruf-Szenario generieren
            //TODO Diese Methode muss immer sofort nach GetMoves() aufgerufen werden
        }
    }
}
