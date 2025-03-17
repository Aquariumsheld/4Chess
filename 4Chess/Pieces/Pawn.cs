using _4Chess.Game;
using System.Numerics;

namespace _4Chess.Pieces
{
    class Pawn : Piece
    {
        /// <summary>
        /// Legt fest, ob die Figur über den gesamten Spielverlauf hinweg schon einmal bewegt wurde.
        /// </summary>
        public bool IsUnmoved { get; set; } = true;

        /// <summary>
        /// Legt fest, ob die Figur gerade mithilfe von EnPassent geschlagen werden kann.
        /// </summary>
        public bool IsEnPassant { get; set; } = false;

        public Pawn(int yPosition, int xPosition, Color alignment, _4ChessGame game)
        {
            Y = yPosition;
            X = xPosition;
            Alignment = alignment;
            FilePath = alignment == Color.White ? "WhitePawn.png" : "BlackPawn.png";
            Game = game;
        }

        /// <summary>
        /// Ermittelt alle für den Bauer möglichen Züge in Abhängigkeit von verbündeten und feindlichen Spielfiguren.
        /// </summary>
        /// <param name="validate">Legt fest, ob die Methode im Rahmen der Methode ValidateMoves() aufgerufen wird. Sollte dies der Fall sein, so wird durch
        /// diesen Wert eine Rekursion vermieden.</param>
        /// <returns>Eine Liste mit allen für die Figur mögliche Züge</returns>
        public override List<Vector2> GetMoves(bool validate = true)
        {
            List<Vector2> moves = new List<Vector2>();

            int yDiff = Alignment switch
            {
                Color.White => -1,
                Color.Black => +1,
                _ => 0
            };

            if (yDiff == 0)
                Console.WriteLine("Der Bauer hat keine Farbe !!!");

            // Ein Feld vorwärts, falls frei
            if (Y + yDiff >= 0 && Y + yDiff < _4ChessGame.BOARD_DIMENSIONS)
            {
                if (Game?.Board[Y + yDiff][X] == null)
                {
                    moves.Add(new Vector2(X, Y + yDiff));
                }
            }

            bool onStartingRow = (Alignment == Color.White && Y == 6) || (Alignment == Color.Black && Y == 1);
            if (IsUnmoved && onStartingRow && Y + yDiff * 2 >= 0 && Y + yDiff * 2 < _4ChessGame.BOARD_DIMENSIONS)
            {
                if (Game?.Board[Y + yDiff][X] == null && Game?.Board[Y + yDiff * 2][X] == null)
                {
                    moves.Add(new Vector2(X, Y + yDiff * 2));
                }
            }

            if (X - 1 >= 0 && Y + yDiff >= 0 && Y + yDiff < _4ChessGame.BOARD_DIMENSIONS)
            {
                var leftCapture = Game?.Board[Y + yDiff][X - 1];
                if (leftCapture != null && leftCapture.Alignment != this.Alignment)
                {
                    moves.Add(new Vector2(X - 1, Y + yDiff));
                }
            }

            if (X + 1 < _4ChessGame.BOARD_DIMENSIONS && Y + yDiff >= 0 && Y + yDiff < _4ChessGame.BOARD_DIMENSIONS)
            {
                var rightCapture = Game?.Board[Y + yDiff][X + 1];
                if (rightCapture != null && rightCapture.Alignment != this.Alignment)
                {
                    moves.Add(new Vector2(X + 1, Y + yDiff));
                }
            }

            moves.AddRange(GetEnPassantMoves());

            if (validate)
                return ValidateMoves(moves);

            else
                return moves;
        }

        public List<Vector2> GetEnPassantMoves()
        {
            List<Vector2> enPassantMoves = new List<Vector2>();
            int yDiff = (Alignment == Color.White) ? -1 : 1;

            // Linker Nachbar
            if (X - 1 >= 0)
            {
                var leftPiece = Game.Board[Y][X - 1];
                if (leftPiece is Pawn enemyPawn && enemyPawn.Alignment != this.Alignment && enemyPawn.IsEnPassant)
                {
                    // Das Zielfeld befindet sich diagonal vorne
                    enPassantMoves.Add(new Vector2(X - 1, Y + yDiff));
                }
            }
            // Rechter Nachbar
            if (X + 1 < _4ChessGame.BOARD_DIMENSIONS)
            {
                var rightPiece = Game.Board[Y][X + 1];
                if (rightPiece is Pawn enemyPawn && enemyPawn.Alignment != this.Alignment && enemyPawn.IsEnPassant)
                {
                    enPassantMoves.Add(new Vector2(X + 1, Y + yDiff));
                }
            }
            return enPassantMoves;
        }

        public bool IsAtEnd()
        {
            bool result = false;

            switch (Alignment)
            {
                case Color.White:
                    if (Y == 0) result = true;
                    else result = false;
                    break;
                case Color.Black:
                    if (Y == 7) result = true;
                    else result = false;
                    break;
                default:
                    result = true;
                    break;
            }

            return result;
        }
    }
}
