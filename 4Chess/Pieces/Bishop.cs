using _4Chess.Game;
using System.Numerics;

namespace _4Chess.Pieces
{
    class Bishop : Piece
    {
        public Bishop(int yPosition, int xPosition, Color alignment, _4ChessGame game)
        {
            Y = yPosition;
            X = xPosition;
            FilePath = alignment == Color.White ? "WhiteBishop.png" : "BlackBishop.png";
            Alignment = alignment;
            Game = game;
        }

        /// <summary>
        /// Ermittelt alle für den Läufer möglichen Züge in Abhängigkeit von verbündeten und feindlichen Spielfiguren.
        /// </summary>
        /// <param name="validate">Legt fest, ob die Methode im Rahmen der Methode ValidateMoves() aufgerufen wird. Sollte dies der Fall sein, so wird durch
        /// diesen Wert eine Rekursion vermieden.</param>
        /// <returns>Eine Liste mit allen für die Figur mögliche Züge</returns>
        public override List<Vector2> GetMoves(bool validate = true, bool rocharde = true)
        {
            bool leftUp = true;
            bool rightUp = true;
            bool leftDown = true;
            bool rightDown = true;


            List<Vector2> moves = [];

            if (Game == null || Game.Board == null)
                return moves;

            for (int i = 1;  i < Game.Board.Count; i++)
            {
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
