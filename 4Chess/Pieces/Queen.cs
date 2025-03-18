using _4Chess.Game;
using System.Numerics;

namespace _4Chess.Pieces
{
    class Queen : Piece
    {
        public Queen(int yPosition, int xPosition, Color alignment, _4ChessGame game)
        {
            Y = yPosition;
            X = xPosition;
            FilePath = alignment == Color.White ? "WhiteQueen.png" : "BlackQueen.png";
            Alignment = alignment;
            Game = game;
        }

        /// <summary>
        /// Ermittelt alle für die Dame möglichen Züge in Abhängigkeit von verbündeten und feindlichen Spielfiguren.
        /// </summary>
        /// <param name="validate">Legt fest, ob die Methode im Rahmen der Methode ValidateMoves() aufgerufen wird. Sollte dies der Fall sein, so wird durch
        /// diesen Wert eine Rekursion vermieden.</param>
        /// <returns>Eine Liste mit allen für die Figur mögliche Züge</returns>
        public override List<Vector2> GetMoves(bool validate = true, bool rocharde = true)
        {
            List<Vector2> moves = [];

            #region Werte für Begrenzungen in die einzelnen Richtungen
            bool left = true;
            bool right = true;
            bool up = true;
            bool down = true;
            bool leftUp = true;
            bool rightUp = true;
            bool leftDown = true;
            bool rightDown = true;
            #endregion

            for (int i = 1; i < Game?.Board.Count; i++)
            {
                // Bewegungen in alle Richtungen
                if(left) left = AddMoveIfValid(moves, X - i, Y); // links
                if(right) right = AddMoveIfValid(moves, X + i, Y); // rechts
                if(up) up = AddMoveIfValid(moves, X, Y - i); // oben
                if(down) down = AddMoveIfValid(moves, X, Y + i); // unten
                if(leftUp) leftUp = AddMoveIfValid(moves, X - i, Y - i); // links oben
                if(rightUp) rightUp = AddMoveIfValid(moves, X + i, Y - i); // rechts oben
                if(leftDown) leftDown = AddMoveIfValid(moves, X - i, Y + i); // links unten
                if(rightDown) rightDown = AddMoveIfValid(moves, X + i, Y + i); // rechts unten
            }

            if (validate)
                return ValidateMoves(moves);

            else
                return moves;
        }
    }
}
