using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _4Chess.Pieces
{
    abstract class Piece
    {
        public string FilePath { get; set; } = "";

        public static (int,int) Indexer { get; set; }

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
    }
}
