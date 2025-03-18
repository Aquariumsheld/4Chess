using _4Chess.Pieces;
using BIERKELLER.BIERRender;
using Raylib_CsLo;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using BIERKELLER.BIERUI;
using System.Collections;
using System.ComponentModel;
using System;
using _4Chess.Game.Move;
using System.IO;
using static Raylib_CsLo.Raylib;
using static System.Net.Mime.MediaTypeNames;
using _4Chess.Game.Multiplayer;

namespace _4Chess.Game.Move
{
    public static class _4ChessMove
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
        public static float CastlingAnimationDuration = 0.5f; // Dauer der Animation (in Sekunden)
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
            // Aktualisiere die Castling-Animation, falls aktiv
            UpdateCastlingAnimation(game);

            if(!MultiplayerManager.IsHost && isWhiteTurn)
                return;

            if(MultiplayerManager.IsHost && !isWhiteTurn)
                return;

            if (!_4ChessGame.continueGame)
                return;

            // Aktualisiere die Mausposition
            MouseRect.x = Raylib.GetMousePosition().X;
            MouseRect.y = Raylib.GetMousePosition().Y;

            if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
            {
                HandleMousePressed(pieces, game);
            }

            if (Raylib.IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT) && DraggedPiece != null)
            {
                HandleMouseReleased(pieces, game);
            }
        }

        /// <summary>
        /// Aktualisiert die Position der Figur bei einer aktiven Castling-Animation.
        /// </summary>
        private static void UpdateCastlingAnimation(_4ChessGame game)
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
        }

        /// <summary>
        /// Behandelt die Logik, wenn die linke Maustaste gedrückt wird (Figurenauswahl und UI-Klicks).
        /// </summary>
        private static void HandleMousePressed(List<Piece> pieces, _4ChessGame game)
        {
            // Überprüfe, ob eine Figur ausgewählt wurde
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

                    // Erzeuge die Anzeige der möglichen Züge
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

            
            // Behandle UI-Komponenten-Klicks
            game.UIComponents.ForEach(c =>
            {
                if (c.CompnentHitboxes.Any(h => Raylib.CheckCollisionRecs(MouseRect, h)))
                {
                    if (c.IsVisible && c.IsClickable)
                        c.ClickEvent.Invoke();
                }
            });

            game.UIComponents.Clear();
        }

        /// <summary>
        /// Behandelt die Logik, wenn die linke Maustaste losgelassen wird (Figurenbewegung, Validierung und Castling).
        /// </summary>
        private static void KingCasteling(_4ChessGame game, int newX, int newY)
        {
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
        }

        private static void SwitchPiece(_4ChessGame game, int y, int x)
        {
            if (DraggedPiece != null)
            {
                game.UIComponents.Add(new BIERButton(" Bishop  ", 0, _4ChessGame.WINDOW_HEIGHT / 2 - 120 * 2 + 30, 350, 120, BLACK, GOLD, spacing: 3));
                game.UIComponents[0].ClickEvent += () =>
                {
                    game.Board[y][x] = new Bishop(y, x, game.Board[y][x].Alignment, game);
                };
                game.UIComponents.Add(new BIERButton(" Rook  ", 0, _4ChessGame.WINDOW_HEIGHT / 2 - 120 + 50, 350, 120, BLACK, GOLD, spacing: 3));
                game.UIComponents[1].ClickEvent += () =>
                {
                    game.Board[y][x] = new Rook(y, x, game.Board[y][x].Alignment, game);
                };
                game.UIComponents.Add(new BIERButton(" Queen  ", 0, _4ChessGame.WINDOW_HEIGHT / 2 + 120 - 50, 350, 120, BLACK, GOLD, spacing: 3));
                game.UIComponents[2].ClickEvent += () =>
                {
                    game.Board[y][x] = new Queen(y, x, game.Board[y][x].Alignment, game);
                };
                game.UIComponents.Add(new BIERButton(" Knight  ", 0, _4ChessGame.WINDOW_HEIGHT / 2 + 120 * 2 - 30, 350, 120, BLACK, GOLD, spacing: 3));
                game.UIComponents[3].ClickEvent += () =>
                {
                    game.Board[y][x] = new Knight(y, x, game.Board[y][x].Alignment, game);
                };
                 
            }
        }

        private static void HandleMouseReleased(List<Piece> pieces, _4ChessGame game)
        {
            int newX = (int)((MouseRect.x - _4ChessGame.BOARDXPos) / _4ChessGame.TILE_SIZE);
            int newY = (int)((MouseRect.y - _4ChessGame.BOARDYPos) / _4ChessGame.TILE_SIZE);
            newX = Math.Clamp(newX, 0, _4ChessGame.BOARD_DIMENSIONS - 1);
            newY = Math.Clamp(newY, 0, _4ChessGame.BOARD_DIMENSIONS - 1);

            // Abbruch, wenn Castling-Ziel getroffen wird
            if (IsCastlingAnimationActive &&
                newX == (int)CastlingRookTarget.X &&
                newY == (int)CastlingRookTarget.Y)
            {
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

                // Prüfe auf King-Bewegungen für Castling
                KingCasteling(game, newX, newY);

                // Aktualisiere den IsUnmoved-Status, falls die Figur sich bewegt hat
                if (DraggedPiece.X != (int)OriginalPosition.X || DraggedPiece.Y != (int)OriginalPosition.Y)
                {
                    if (DraggedPiece is Pawn pawn)
                    {
                        int diff = DraggedPiece.Y - (int)OriginalPosition.Y;
                        if (diff == 2 || diff == -2)
                            pawn.IsEnPassant = true;
                        else
                            pawn.IsEnPassant = false;

                        pawn.IsUnmoved = false;
                    }
                    if (DraggedPiece is King king) king.IsUnmoved = false;
                    if (DraggedPiece is Rook rook) rook.IsUnmoved = false;
                }

                if (DraggedPiece is Pawn pawnPassant)
                {
                    if (pawnPassant.X - (int)OriginalPosition.X == 1 || pawnPassant.X - (int)OriginalPosition.X == -1)
                    {
                        int diffy = pawnPassant.Y - 1;
                        if (diffy >= 0)
                        {
                            if (game.Board[pawnPassant.Y - 1][pawnPassant.X] is Pawn pawn5 && pawn5.IsEnPassant)
                                game.Board[pawnPassant.Y - 1][pawnPassant.X] = null;

                            else if (game.Board[pawnPassant.Y + 1][pawnPassant.X] is Pawn pawn6 && pawn6.IsEnPassant)
                                game.Board[pawnPassant.Y + 1][pawnPassant.X] = null;
                        }
                    }
                }

                if (DraggedPiece is Pawn pawn2 && pawn2.IsAtEnd())
                {
                    SwitchPiece(game, DraggedPiece.Y, DraggedPiece.X);
                }

                if (_4ChessGame.MultiplayerMode)
                {
                    string msg = $"MOVE {(int)OriginalPosition.X} {(int)OriginalPosition.Y} {newX} {newY}";
                    System.Threading.Tasks.Task.Run(async () => {
                        await MultiplayerManager.SendMessageAsync(msg);
                    });
                    _4ChessGame.IsLocalTurn = false;
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
