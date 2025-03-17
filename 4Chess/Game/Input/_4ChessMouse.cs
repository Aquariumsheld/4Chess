using _4Chess.Pieces;
using BIERKELLER.BIERRender;
using Raylib_CsLo;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

namespace _4Chess.Game.Input
{
    public static class _4ChessMouse
    {
        public static readonly Color PossibleMoveTileColor = Raylib.ColorAlpha(Raylib.ColorFromHSV(113f, 0.83f, 0.64f), 0.4f);
        public static readonly Color PossibleTakePieceTileColor = Raylib.ColorAlpha(Raylib.ColorFromHSV(352f, 0.83f, 0.64f), 0.4f);
        public static Rectangle MouseRect = new(0, 0, 1, 1);
        public static Piece? DraggedPiece = null;
        public static Vector2 OriginalPosition = Vector2.Zero;

        private static int moveCounter = 1;
        private static bool isWhiteTurn = true;

        public static List<BIERRenderRect> PossibleMoveRenderTiles { get; set; } = new List<BIERRenderRect>();

        // --- Variablen für die Castling-Animation ---
        public static bool IsCastlingAnimationActive = false;
        public static Piece? CastlingRook = null;
        public static Vector2 CastlingRookStart = Vector2.Zero;
        public static Vector2 CastlingRookTarget = Vector2.Zero;
        public static float CastlingAnimationTimer = 0f;
        public static float CastlingAnimationDuration = 4.5f; // Dauer der Animation (in Sekunden)
        public static Vector2 AnimatedCastlingRookPos = Vector2.Zero;
        // ------------------------------------------------

        /// <summary>
        /// Wechselt im Alterniermodus (wenn debugMoveMode false) die Zugfarbe nach jedem gültigen Zug.
        /// </summary>
        public static void TurnChange()
        {
            if (!_4ChessGame.debugMoveMode)
            {
                isWhiteTurn = !isWhiteTurn;
            }
            else
            {
                if (moveCounter % 2 == 0)
                    isWhiteTurn = !isWhiteTurn;
            }
        }

        public static void MouseUpdate(List<Piece> pieces, _4ChessGame game)
        {
            if (IsCastlingAnimationActive && CastlingRook != null)
            {
                float dt = Raylib.GetFrameTime();
                CastlingAnimationTimer += dt;
                float t = Math.Min(CastlingAnimationTimer / CastlingAnimationDuration, 1.0f);

                Vector2 newRookPos = new Vector2(
                    CastlingRookStart.X + t * (CastlingRookTarget.X - CastlingRookStart.X),
                    CastlingRookStart.Y + t * (CastlingRookTarget.Y - CastlingRookStart.Y)
                );

                AnimatedCastlingRookPos = newRookPos;

                if (t >= 1.0f)
                {
                    CastlingRook.X = (int)CastlingRookTarget.X;
                    CastlingRook.Y = (int)CastlingRookTarget.Y;
                    game.Board[(int)CastlingRookTarget.Y][(int)CastlingRookTarget.X] = CastlingRook;
                    IsCastlingAnimationActive = false;
                    CastlingRook = null;
                }
            }


            if (!_4ChessGame.continueGame)
                return;

            MouseRect.x = Raylib.GetMousePosition().X;
            MouseRect.y = Raylib.GetMousePosition().Y;

            if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
            {
                foreach (var piece in pieces)
                {
                    if (!_4ChessGame.debugMoveMode && piece.Alignment != (isWhiteTurn ? Piece.Color.White : Piece.Color.Black))
                        continue;

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

                if (IsCastlingAnimationActive &&
                newX == (int)_4ChessMouse.CastlingRookTarget.X &&
                newY == (int)_4ChessMouse.CastlingRookTarget.Y)
                {
                    // Option: Ausgabe einer Nachricht oder einfach Abbruch des Zugs
                    DraggedPiece.X = (int)OriginalPosition.X;
                    DraggedPiece.Y = (int)OriginalPosition.Y;
                    DraggedPiece = null;
                    PossibleMoveRenderTiles.Clear();
                    return;
                }

                Vector2 newPos = new(newX, newY);
                bool isValidMove = DraggedPiece.GetMoves().Any(move => (int)move.X == newX && (int)move.Y == newY);
                if (isValidMove)
                {
                    DraggedPiece.X = newX;
                    DraggedPiece.Y = newY;

                    game.Board[(int)OriginalPosition.Y][(int)OriginalPosition.X] = null;
                    game.Board[newY][newX] = DraggedPiece;

                    if (DraggedPiece.GetType() == typeof(King))
                    {
                        int diff = DraggedPiece.X - (int)OriginalPosition.X;
                        if (diff == 2)
                        {
                            CastlingRook = game.Board[DraggedPiece.Y][7];
                            game.Board[DraggedPiece.Y][7] = null; 
                            if (CastlingRook == null)
                            {
                                CastlingRook = new Rook(DraggedPiece.Y, 7, DraggedPiece.Alignment, game);
                            }
                            CastlingRookStart = new Vector2(7, DraggedPiece.Y);
                            CastlingRookTarget = new Vector2(5, DraggedPiece.Y);
                            CastlingAnimationTimer = 0f;
                            IsCastlingAnimationActive = true;
                        }
                        else if (diff == -2)
                        {
                            CastlingRook = game.Board[DraggedPiece.Y][0];
                            game.Board[DraggedPiece.Y][0] = null;
                            if (CastlingRook == null)
                            {
                                CastlingRook = new Rook(DraggedPiece.Y, 0, DraggedPiece.Alignment, game);
                            }
                            CastlingRookStart = new Vector2(0, DraggedPiece.Y);
                            CastlingRookTarget = new Vector2(3, DraggedPiece.Y);
                            CastlingAnimationTimer = 0f;
                            IsCastlingAnimationActive = true;
                        }
                        switch (DraggedPiece.Alignment)
                        {
                            case Piece.Color.Black:
                                game.BlackKingPosition = new Vector2(newX, newY);
                                break;
                            case Piece.Color.White:
                                game.WhiteKingPosition = new Vector2(newX, newY);
                                break;
                        }
                    }

                    if (DraggedPiece.X != (int)OriginalPosition.X && DraggedPiece.Y != (int)OriginalPosition.Y)
                    {
                        if (DraggedPiece is Pawn pawn) pawn.IsUnmoved = false;
                        if (DraggedPiece is King king) king.IsUnmoved = false;
                        if (DraggedPiece is Rook rook) rook.IsUnmoved = false;
                    }

                    moveCounter++;
                    TurnChange();
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
}
