using Raylib_CsLo;
using _4Chess.Pieces;
using System.Collections.Generic;
using _4Chess.Game;

namespace BIERKELLER.BIERInputs
{
    public static class BIERMouse
    {
        public static Rectangle MouseRect = new Rectangle(0, 0, 1, 1);
        public static Piece DraggedPiece = null;

        // Aktualisiert den Mauszeiger und überprüft, ob ein Piece ausgewählt oder losgelassen wurde
        public static void MouseUpdate(List<Piece> pieces)
        {
            // Aktualisiere die Mausposition
            MouseRect.x = Raylib.GetMousePosition().X;
            MouseRect.y = Raylib.GetMousePosition().Y;

            // Beim ersten Klick: Prüfe, ob eine Figur angeklickt wurde
            if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
            {
                foreach (var piece in pieces)
                {
                    // Berechne die Hitbox des Pieces auf Basis der Rasterposition
                    Rectangle hitbox = new Rectangle(piece.X * _4ChessGame.TILE_SIZE + _4ChessGame.BOARDXPos,
                                                      piece.Y * _4ChessGame.TILE_SIZE + _4ChessGame.BOARDYPos,
                                                      _4ChessGame.TILE_SIZE,
                                                      _4ChessGame.TILE_SIZE);
                    if (Raylib.CheckCollisionRecs(hitbox, MouseRect))
                    {
                        DraggedPiece = piece;
                        break;
                    }
                }
            }

            // Beim Loslassen: Bestimme den Ziel-Rasterplatz anhand der Mausposition
            if (Raylib.IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT) && DraggedPiece != null)
            {
                int newX = (int)((MouseRect.x - _4ChessGame.BOARDXPos) / _4ChessGame.TILE_SIZE);
                int newY = (int)((MouseRect.y - _4ChessGame.BOARDYPos) / _4ChessGame.TILE_SIZE);
                newX = Math.Clamp(newX, 0, _4ChessGame.BOARD_DIMENSIONS - 1);
                newY = Math.Clamp(newY, 0, _4ChessGame.BOARD_DIMENSIONS - 1);

                DraggedPiece.X = newX;
                DraggedPiece.Y = newY;
                DraggedPiece = null;
            }
        }
    }
}
