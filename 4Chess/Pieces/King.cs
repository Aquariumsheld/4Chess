﻿using System.Numerics;
using _4Chess.Game;

namespace _4Chess.Pieces
{
    class King : Piece
    {
        /// <summary>
        /// Legt fest, ob die Figur im gesamten Spielverlauf schon einmal bewegt wurde
        /// </summary>
        public bool IsUnmoved { get; set; } = true;

        public King(int yPosition, int xPosition, Color alignment, _4ChessGame game)
        {
            Y = yPosition;
            X = xPosition;
            Alignment = alignment;
            FilePath = alignment == Color.White ? "WhiteKing.png" : "BlackKing.png";
            Game = game;

            switch (Alignment)
            {
                case Color.Black:
                    game.BlackKingPosition = new Vector2(xPosition, yPosition);
                    break;
                case Color.White:
                    game.WhiteKingPosition = new Vector2(xPosition, yPosition);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Ermittelt alle für den König möglichen Züge in Abhängigkeit von verbündeten und feindlichen Spielfiguren.
        /// </summary>
        /// <param name="validate">Legt fest, ob die Methode im Rahmen der Methode ValidateMoves() aufgerufen wird. Sollte dies der Fall sein, so wird durch
        /// diesen Wert eine Rekursion vermieden.</param>
        /// <returns>Eine Liste mit allen für die Figur mögliche Züge</returns>

        public override List<Vector2> GetMoves(bool validate = true, bool rocharde = true)
        {
            List<Vector2> moves = [];

            if (Game == null || Game.Board == null)
                return moves;

            // Rocharde
            if (rocharde)
            {
                (bool DoRocharde, float moveLeft, float moveRight) = Rocharde(this);
                if (DoRocharde)
                {
                    moves.Add(new Vector2(X + moveLeft * 2, Y));
                    moves.Add(new Vector2(X + moveRight * 2, Y));
                }
            }

            // Bewegungen in alle Richtungen
            AddMoveIfValid(moves, X - 1, Y); // links
            AddMoveIfValid(moves, X + 1, Y); // rechts
            AddMoveIfValid(moves, X, Y - 1); // oben
            AddMoveIfValid(moves, X, Y + 1); // unten
            AddMoveIfValid(moves, X - 1, Y - 1); // links oben
            AddMoveIfValid(moves, X + 1, Y - 1); // rechts oben
            AddMoveIfValid(moves, X - 1, Y + 1); // links unten
            AddMoveIfValid(moves, X + 1, Y + 1); // rechts unten

            if (validate)
                return ValidateMoves(moves);

            return moves;
        }

        internal (bool, float, float) Rocharde(King king)
        {
            List<Vector2> enemyMoves = GetAllEnemyMoves();
            bool rocharde = false;
            int rochardeLeft = 0;
            int rochardeRight = 0;
            switch (Alignment)
            {
                case Color.Black:
                    if (king.IsUnmoved && Game?.Board[0][0] is Rook rookLeft1 && rookLeft1.IsUnmoved)
                    {
                        if (Game.Board[0][1] == null && Game.Board[0][2] == null && Game.Board[0][3] == null &&
                            !enemyMoves.Contains(new Vector2(X, Y)) &&
                            !enemyMoves.Contains(new Vector2(0, 0)) &&
                            !enemyMoves.Contains(new Vector2(1, 0)) &&
                            !enemyMoves.Contains(new Vector2(2, 0)))
                        {
                            rocharde = true;
                            rochardeLeft = -1;
                        }
                    }

                    if (king.IsUnmoved && Game?.Board[0][7] is Rook rookRight1 && rookRight1.IsUnmoved)
                    {
                        if (Game.Board[0][5] == null && Game.Board[0][6] == null &&
                            !enemyMoves.Contains(new Vector2(X, Y)) &&
                            !enemyMoves.Contains(new Vector2(4, 0)) &&
                            !enemyMoves.Contains(new Vector2(5, 0)) &&
                            !enemyMoves.Contains(new Vector2(6, 0)) &&
                            !enemyMoves.Contains(new Vector2(7, 0)))
                        {
                            rocharde = true;
                            rochardeRight = 1;
                        }
                    }
                    break;
                case Color.White:
                    if (king.IsUnmoved && Game?.Board[7][0] is Rook rookLeft2 && rookLeft2.IsUnmoved)
                    {
                        if (Game.Board[7][1] == null && Game.Board[7][2] == null && Game.Board[7][3] == null &&
                            !enemyMoves.Contains(new Vector2(X, Y)) &&
                            !enemyMoves.Contains(new Vector2(7, 7)) &&
                            !enemyMoves.Contains(new Vector2(1, 7)) &&
                            !enemyMoves.Contains(new Vector2(2, 7)))
                        {
                            rocharde = true;
                            rochardeLeft = -1;
                        }
                    }

                    if (Game?.Board[7][5] == null && Game?.Board[7][6] == null && king.IsUnmoved && Game?.Board[7][7] is Rook rookRight2 && rookRight2.IsUnmoved)
                    {
                        if (
                            !enemyMoves.Contains(new Vector2(X, Y)) &&
                            !enemyMoves.Contains(new Vector2(4, 7)) &&
                            !enemyMoves.Contains(new Vector2(5, 7)) &&
                            !enemyMoves.Contains(new Vector2(6, 7)) &&
                            !enemyMoves.Contains(new Vector2(7, 7)))
                        {
                            rocharde = true;
                            rochardeRight = 1;
                        }
                    }
                    break;
                default:
                    break;
            }
            return (rocharde, rochardeLeft, rochardeRight);
        }      
    }
}
