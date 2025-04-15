using _4Chess.Game.Multiplayer;
using _4Chess.Pieces;
using BIERKELLER.BIERRender;
using BIERKELLER.BIERUI;
using Raylib_CsLo;
using System.Numerics;
using static Raylib_CsLo.Raylib;

namespace _4Chess.Game.Input;

public static class _4ChessMove
{
    public static readonly Color PossibleMoveTileColor = ColorAlpha(ColorFromHSV(113f, 0.83f, 0.64f), 0.4f);
    public static readonly Color PossibleTakePieceTileColor = ColorAlpha(ColorFromHSV(352f, 0.83f, 0.64f), 0.4f);
    public static Rectangle MouseRect = new(0, 0, 1, 1);
    public static Piece? DraggedPiece = null;
    public static Vector2 OriginalPosition = Vector2.Zero;

    private static int moveCounter = 1;
    private static bool isWhiteTurn = true;

    public static List<BIERRenderRect> PossibleMoveRenderTiles { get; set; } = new List<BIERRenderRect>();

    // --- Variablen für die Castling-Animation ---
    public static Piece? CastlingRook = null;
    public static Vector2 CastlingRookStart = Vector2.Zero;
    public static Vector2 CastlingRookTarget = Vector2.Zero;
    // ------------------------------------------------

    /// <summary>
    /// Wechselt im Alterniermodus (wenn debugMoveMode false) die Zugfarbe nach jedem gültigen Zug.
    /// </summary>
    public static void TurnChange()
    {
        isWhiteTurn = !isWhiteTurn;
    }

    public static void MouseUpdate(List<Piece> pieces, _4ChessGame game)
    {
        if (!_4ChessGame.IsLocalTurn)
            return;

        if (!_4ChessGame.continueGame)
            return;

        // Aktualisiere die Mausposition
        MouseRect.x = GetMousePosition().X;
        MouseRect.y = GetMousePosition().Y;

        if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
        {
            HandleMousePressed(pieces, game);
        }

        if (IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT) && DraggedPiece != null)
        {
            HandleMouseReleased(game);
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

            if (!_4ChessGame.debugMoveMode && _4ChessGame.MultiplayerMode && !_4ChessGame.IsLocalTurn)
                continue;

            Rectangle hitbox = new(
                piece.X * _4ChessGame.TILE_SIZE + _4ChessGame.BOARDXPos,
                piece.Y * _4ChessGame.TILE_SIZE + _4ChessGame.BOARDYPos,
                _4ChessGame.TILE_SIZE,
                _4ChessGame.TILE_SIZE
            );
            if (CheckCollisionRecs(hitbox, MouseRect))
            {
                game.IpInput.Hide();
                game.UIComponents["JoinGameBtn"].Hide();
                game.UIComponents["HostGameBtn"].Hide();
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
        game.UIComponents.Values.ToList().ForEach(c =>
        {
            if (c.CompnentHitboxes.Any(h => CheckCollisionRecs(MouseRect, h)))
            {
                if (c.IsVisible && c.IsClickable)
                    c.ClickEvent.Invoke();
            }
        });
    }

    /// <summary>
    /// Behandelt die Logik, wenn die linke Maustaste losgelassen wird (Figurenbewegung, Validierung und Castling).
    /// </summary>
    private static void KingCasteling(_4ChessGame game, int newX, int newY)
    {
        if (DraggedPiece?.GetType() == typeof(King))
        {
            int diff = DraggedPiece.X - (int)OriginalPosition.X;
            if (diff == 2)
            {
                CastlingRook = game.Board[DraggedPiece.Y][7];
                game.Board[DraggedPiece.Y][7] = null;
                if (CastlingRook != null)
                {
                    game.Board[DraggedPiece.Y][7 - 2] = new Rook(DraggedPiece.Y, 7 - 2, DraggedPiece.Alignment, game);
                }
                CastlingRookStart = new Vector2(7, DraggedPiece.Y);
                CastlingRookTarget = new Vector2(5, DraggedPiece.Y);
            }
            else if (diff == -2)
            {
                CastlingRook = game.Board[DraggedPiece.Y][0];
                game.Board[DraggedPiece.Y][0] = null;
                if (CastlingRook != null)
                {
                    game.Board[DraggedPiece.Y][0 + 3] = new Rook(DraggedPiece.Y, 0 + 3, DraggedPiece.Alignment, game);
                }
                CastlingRookStart = new Vector2(0, DraggedPiece.Y);
                CastlingRookTarget = new Vector2(3, DraggedPiece.Y);
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
        //TODO Sehr unwarscheinlicher bug wenn man 2 Bauern hinten hat
        var piece = game.Board[y][x];
        if (DraggedPiece != null && piece != null)
        {
            int z = 0;
            foreach (var e in new List<string>() { "SelectBishopBtn", "SelectRookBtn", "SelectQueenBtn", "SelectKnightBtn" })
            {
                if (game.UIComponents.TryGetValue(e, out BIERUIComponent? value))
                {
                    value.Show();
                }
                else
                {
                    game.UIComponents.Add(e, new BIERButton($" {e.Replace("Select", "").Replace("Btn", "")}  ", 0, _4ChessGame.WINDOW_HEIGHT / 2 - 120 * 2 + 30 + z, 350, 120, BLACK, GOLD, spacing: 3));
                    game.UIComponents[e].ClickEvent += () =>
                    {
                        game.Board[y][x] = e.Replace("Select", "").Replace("Btn", "") switch
                        {
                            "Bishop" => new Bishop(y, x, piece.Alignment, game),
                            "Rook" => new Rook(y, x, piece.Alignment, game),
                            "Queen" => new Queen(y, x, piece.Alignment, game),
                            "Knight" => new Knight(y, x, piece.Alignment, game),
                            _ => new Queen(y, x, piece.Alignment, game),
                        };
                        HideAllSelectBtns(game);
                    };
                }
                z += 150;
            }
        }
    }

    private static void HideAllSelectBtns(_4ChessGame game)
    {
        game.UIComponents["SelectBishopBtn"].Hide();
        game.UIComponents["SelectRookBtn"].Hide();
        game.UIComponents["SelectQueenBtn"].Hide();
        game.UIComponents["SelectKnightBtn"].Hide();
    }

    private static void HandleMouseReleased(_4ChessGame game)
    {
        int newX = (int)((MouseRect.x - _4ChessGame.BOARDXPos) / _4ChessGame.TILE_SIZE);
        int newY = (int)((MouseRect.y - _4ChessGame.BOARDYPos) / _4ChessGame.TILE_SIZE);
        newX = Math.Clamp(newX, 0, _4ChessGame.BOARD_DIMENSIONS - 1);
        newY = Math.Clamp(newY, 0, _4ChessGame.BOARD_DIMENSIONS - 1);

        // Abbruch, wenn Castling-Ziel getroffen wird
        if (
            newX == (int)CastlingRookTarget.X &&
            newY == (int)CastlingRookTarget.Y &&
            DraggedPiece != null
        )
        {
            DraggedPiece.X = (int)OriginalPosition.X;
            DraggedPiece.Y = (int)OriginalPosition.Y;
            DraggedPiece = null;
            PossibleMoveRenderTiles.Clear();
            return;
        }

        Vector2 newPos = new(newX, newY);
        bool isValidMove = (DraggedPiece?.GetMoves().Any(move => (int)move.X == newX && (int)move.Y == newY) ?? false);
        if (isValidMove && DraggedPiece != null)
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
                string msg = $"{MoveCounter.SerializeBoard(game.Board)}";
                //string msg = $"MOVE {(int)OriginalPosition.X} {(int)OriginalPosition.Y} {newX} {newY}";
                Task.Run(async () =>
                {
                    await MultiplayerManager.SendMessageAsync(msg);
                });
                _4ChessGame.IsLocalTurn = false;
            }
            moveCounter++;
            TurnChange();
        }
        else if (DraggedPiece != null)
        {
            DraggedPiece.X = (int)OriginalPosition.X;
            DraggedPiece.Y = (int)OriginalPosition.Y;
        }
        DraggedPiece = null;
        PossibleMoveRenderTiles.Clear();
    }
}
