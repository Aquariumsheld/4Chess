using _4Chess.Pieces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace _4Chess
{
    class TempGame
    {
        public static List<List<Piece?>> Board { get; set; } = [];

        public static List<Piece> AllPieces { get; set; } = [];

        public static Vector2 WhiteKingPosition { get; set; }
        public static Vector2 BlackKingPosition { get; set; }
    }
}
