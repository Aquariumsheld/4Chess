using _4Chess.Pieces;
using BIERKELLER.BIERRender;
using Raylib_CsLo;
using System.Numerics;

namespace _4Chess.Game.Input;

public static class _4ChessMouse
{
    public static readonly Color PossibleMoveTileColor = Raylib.ColorAlpha(Raylib.ColorFromHSV(113f, 0.83f, 0.64f), 0.4f);
    public static readonly Color PossibleTakePieceTileColor = Raylib.ColorAlpha(Raylib.ColorFromHSV(352f, 0.83f, 0.64f), 0.4f);
    public static Rectangle MouseRect = new Rectangle(0, 0, 1, 1);
    public static Piece? DraggedPiece = null;
    public static Vector2 OriginalPosition = Vector2.Zero;
    public static List<BIERRenderRect> PossibleMoveRenderTiles { get; set; } = [];

    public static void MouseUpdate(List<Piece> pieces, _4ChessGame game)
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

                    DraggedPiece.GetMoves().ForEach(m =>
                    {
                        PossibleMoveRenderTiles.Add(new BIERRenderRect(
                            m.X * _4ChessGame.TILE_SIZE + _4ChessGame.BOARDXPos,
                            m.Y * _4ChessGame.TILE_SIZE + _4ChessGame.BOARDYPos,
                            _4ChessGame.TILE_SIZE,
                            _4ChessGame.TILE_SIZE,
                            pieces.Any(p => p.X == m.X && p.Y == m.Y) ? PossibleTakePieceTileColor : PossibleMoveTileColor
                        ));
                    });
                    
                    break;
                }
            }

            game.UIComponents.ForEach(c =>
            {
                if (c.CompnentHitboxes.Any(h => Raylib.CheckCollisionRecs(MouseRect, h)))
                {
                    if (c.IsVisible && c.IsClickable)
                        c.ClickEvent.Invoke();
                }
            });
        }

        if (Raylib.IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT) && DraggedPiece != null)
        {
            int newX = (int)((MouseRect.x - _4ChessGame.BOARDXPos) / _4ChessGame.TILE_SIZE);
            int newY = (int)((MouseRect.y - _4ChessGame.BOARDYPos) / _4ChessGame.TILE_SIZE);
            newX = Math.Clamp(newX, 0, _4ChessGame.BOARD_DIMENSIONS - 1);
            newY = Math.Clamp(newY, 0, _4ChessGame.BOARD_DIMENSIONS - 1);

            Vector2 newPos = new(newX, newY);  //Kann zum Testen True gesetztwerden (isValidMove = true)
            bool isValidMove = DraggedPiece.GetMoves().Any(move => (int)move.X == newX && (int)move.Y == newY);
            if (isValidMove)
            {
                DraggedPiece.X = newX;
                DraggedPiece.Y = newY;

                game.Board[(int)OriginalPosition.Y][(int)OriginalPosition.X] = null;
                game.Board[newY][newX] = DraggedPiece;

                if (DraggedPiece.GetType() == typeof(King))
                {
                    switch (DraggedPiece.Alignment)
                    {
                        case Piece.Color.Black:
                            game.BlackKingPosition = new Vector2(newX, newY);
                            break;
                        case Piece.Color.White:
                            game.WhiteKingPosition = new Vector2(newX, newY);
                            break;
                        default:
                            break;
                    }
                }
            }
            else
            {
                DraggedPiece.X = (int)OriginalPosition.X;
                DraggedPiece.Y = (int)OriginalPosition.Y;
            }
            DraggedPiece = null;
            PossibleMoveRenderTiles.Clear();
        }
    }
}
