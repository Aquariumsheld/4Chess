using _4Chess.Game;
using System.Numerics;

namespace _4Chess.Pieces
{
    class Rook : Piece
    {
        /// <summary>
        /// Legt fest, ob die Figur über den gesamten Spielverlauf hinweg schon einmal bewegt wurde.
        /// </summary>
        public bool IsUnmoved { get; set; } = true;

        public Rook(int yPosition, int xPosition, Color alignment, _4ChessGame game)
        {
            Y = yPosition;
            X = xPosition;
            Alignment = alignment;
            FilePath = alignment == Color.White ? "WhiteRook.png" : "BlackRook.png";
            Game = game;
        }

        /// <summary>
        /// Ermittelt alle für den Turm möglichen Züge in Abhängigkeit von verbündeten und feindlichen Spielfiguren.
        /// </summary>
        /// <param name="validate">Legt fest, ob die Methode im Rahmen der Methode ValidateMoves() aufgerufen wird. Sollte dies der Fall sein, so wird durch
        /// diesen Wert eine Rekursion vermieden.</param>
        /// <returns>Eine Liste mit allen für die Figur mögliche Züge</returns>
        public override List<Vector2> GetMoves(bool validate = true, bool rocharde = true)
        {
            List<Vector2> moves = [];

            bool left = true;
            bool right = true;
            bool up = true;
            bool down = true;

            for(int i = 1; i < Game?.Board.Count; i++)
            {
                // Bewegungen in alle Richtungen
                if(left) left = AddMoveIfValid(moves, X - i, Y); // links
                if(right) right = AddMoveIfValid(moves, X + i, Y); // rechts
                if(up) up = AddMoveIfValid(moves, X, Y - i); // oben
                if(down) down = AddMoveIfValid(moves, X, Y + i); // unten
            }

            if (validate)
                return ValidateMoves(moves);

            else
                return moves;
        }
    }
}
