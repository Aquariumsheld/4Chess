using Raylib_CsLo;
using _4Chess.Pieces;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using _4Chess.Game;

namespace BIERKELLER.BIERInputs
{
    public static class BIERMouse
    {
        public static Rectangle MouseRect = new Rectangle(0, 0, 1, 1);
        public static Piece DraggedPiece = null;
        public static Vector2 OriginalPosition = Vector2.Zero;

        public static void MouseUpdate(List<Piece> pieces)
        {
            MouseRect.x = Raylib.GetMousePosition().X;
            MouseRect.y = Raylib.GetMousePosition().Y;

            if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
            {
                foreach (var piece in pieces)
                {
                    Rectangle hitbox = new Rectangle(
                        piece.X * _4ChessGame.TILE_SIZE + _4ChessGame.BOARDXPos,
                        piece.Y * _4ChessGame.TILE_SIZE + _4ChessGame.BOARDYPos,
                        _4ChessGame.TILE_SIZE,
                        _4ChessGame.TILE_SIZE
                    );
                    if (Raylib.CheckCollisionRecs(hitbox, MouseRect))
                    {
                        DraggedPiece = piece;
                        OriginalPosition = new Vector2(piece.X, piece.Y); 
                        break;
                    }
                }
            }

            if (Raylib.IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT) && DraggedPiece != null)
            {
                int newX = (int)((MouseRect.x - _4ChessGame.BOARDXPos) / _4ChessGame.TILE_SIZE);
                int newY = (int)((MouseRect.y - _4ChessGame.BOARDYPos) / _4ChessGame.TILE_SIZE);
                newX = Math.Clamp(newX, 0, _4ChessGame.BOARD_DIMENSIONS - 1);
                newY = Math.Clamp(newY, 0, _4ChessGame.BOARD_DIMENSIONS - 1);

                Vector2 newPos = new Vector2(newX, newY);
                bool isValidMove = DraggedPiece.PossibleMoves.Any(move => (int)move.X == newX && (int)move.Y == newY);
                if (isValidMove)
                {
                    DraggedPiece.X = newX;
                    DraggedPiece.Y = newY;
                }
                else
                {
                    DraggedPiece.X = (int)OriginalPosition.X;
                    DraggedPiece.Y = (int)OriginalPosition.Y;
                }
                DraggedPiece = null;
            }
        }
    }
}
