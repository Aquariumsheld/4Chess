using _4Chess.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace _4Chess.Pieces
{
    class Knight : Piece
    {
        public Knight(int yPosition, int xPosition, Color alignment, _4ChessGame game)
        {
            Y = yPosition;
            X = xPosition;
            Alignment = alignment;
            FilePath = alignment == Color.White ? "WhiteKnight.png" : "BlackKnight.png";
            Game = game;
        }

        /// <summary>
        /// Ermittelt alle für den Springer möglichen Züge in Abhängigkeit von verbündeten und feindlichen Spielfiguren.
        /// </summary>
        /// <param name="validate">Legt fest, ob die Methode im Rahmen der Methode ValidateMoves() aufgerufen wird. Sollte dies der Fall sein, so wird durch
        /// diesen Wert eine Rekursion vermieden.</param>
        /// <returns>Eine Liste mit allen für die Figur mögliche Züge</returns>
        public override List<Vector2> GetMoves(bool validate = true, bool rocharde = true)
        {
            List<Vector2> moves = [];

            if (Game == null || Game.Board == null)
                return moves;

            AddMoveIfValid(moves, X - 1, Y - 2);
            AddMoveIfValid(moves, X + 1, Y - 2);
            AddMoveIfValid(moves, X - 2, Y - 1);
            AddMoveIfValid(moves, X + 2, Y - 1);
            AddMoveIfValid(moves, X - 2, Y + 1);
            AddMoveIfValid(moves, X + 2, Y + 1);
            AddMoveIfValid(moves, X - 1, Y + 2);
            AddMoveIfValid(moves, X + 1, Y + 2);

            if (validate)
                return ValidateMoves(moves);

            else
                return moves;
        }
    }
}
