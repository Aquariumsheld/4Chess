﻿using _4Chess.Game.Input;
using _4Chess.Game.Multiplayer;
using _4Chess.Pieces;
using BIERKELLER.BIERGaming;
using BIERKELLER.BIERRender;
using BIERKELLER.BIERUI;
using Raylib_CsLo;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Text;
using static Raylib_CsLo.Raylib;
using static System.Net.Mime.MediaTypeNames;

namespace _4Chess.Game;

public class _4ChessGame : BIERGame
{
    //Render Werte
    public const int WINDOW_WIDTH = 1920;
    public const int WINDOW_HEIGHT = 1200;
    public const int BOARD_DIMENSIONS = 8;
    public static readonly int BOARDXPos = WINDOW_WIDTH / 4;
    public static readonly int BOARDYPos = WINDOW_HEIGHT / 8;
    public static readonly int TILE_SIZE = (WINDOW_WIDTH - (BOARDXPos * 2)) / BOARD_DIMENSIONS;
    public static readonly Color SELECT_COLOR = ColorFromHSV(157f, 27f, 63f);
    private static bool gameEnds = false;
    private static readonly KeyboardKey[] restartKeys = [KeyboardKey.KEY_ENTER, KeyboardKey.KEY_SPACE, KeyboardKey.KEY_R];
    public static bool continueGame = true;
    public BIERInput IpInput = default!;
    private bool schachmatt = false;


    private Raylib_CsLo.Font _romulusFont;
    public List<BIERRenderObject> RenderObjects { get; set; } = [];
    public Dictionary<string, BIERUIComponent> UIComponents { get; set; } = [];
    public List<List<Piece?>> Board { get; set; } = [];

    private Dictionary<string, Texture> _pieceTextureDict = [];

    public static bool MultiplayerMode = false;
    public static Piece.Color LocalPlayerColor = Piece.Color.White;
    public static bool IsLocalTurn { get; set; } = true;
    private bool multiplayerMenuActive = true;


    //=========DEBUG-VARS===============
    private bool _debugUiHitboxes = false; //F1
    public static bool debugMoveMode = false; //F2
    //==================================

    public Vector2 WhiteKingPosition { get; set; }
    public Vector2 BlackKingPosition { get; set; }

    public _4ChessGame()
    {
        CustomPreRenderFuncs.Add(RenderBoard);
        CustomPostRenderFuncs.Add(RenderPossibleMoveRenderTiles);
        CustomPostRenderFuncs.Add(RenderDraggedPiece);
        CustomPostRenderFuncs.Add(RenderUIComponents);
        CustomPostRenderFuncs.Add(RenderIpInput);
    }

    public override unsafe void GameInit()
    {
        BIERRenderer.Init(WINDOW_WIDTH, WINDOW_HEIGHT, "4Chess", vsync: true, iconPath: "res/rayicon.png");

        // BIERRender-Objekte erst nach BIERRenderer.Init initialisieren, da sie den GL-Context brauchen!

        _pieceTextureDict = new Dictionary<string, Texture>()
        {
             { "WhiteBishop.png", BIERRenderTexture.ResizeTexture(LoadTexture("res/WhiteBishop.png"), TILE_SIZE, TILE_SIZE) },
             { "BlackBishop.png", BIERRenderTexture.ResizeTexture(LoadTexture("res/BlackBishop.png"), TILE_SIZE, TILE_SIZE) },
             { "WhiteKing.png", BIERRenderTexture.ResizeTexture(LoadTexture("res/WhiteKing.png"), TILE_SIZE, TILE_SIZE) },
             { "BlackKing.png", BIERRenderTexture.ResizeTexture(LoadTexture("res/BlackKing.png"), TILE_SIZE, TILE_SIZE) },
             { "WhiteKnight.png", BIERRenderTexture.ResizeTexture(LoadTexture("res/WhiteKnight.png"), TILE_SIZE, TILE_SIZE) },
             { "BlackKnight.png", BIERRenderTexture.ResizeTexture(LoadTexture("res/BlackKnight.png"), TILE_SIZE, TILE_SIZE) },
             { "WhitePawn.png", BIERRenderTexture.ResizeTexture(LoadTexture("res/WhitePawn.png"), TILE_SIZE, TILE_SIZE) },
             { "BlackPawn.png", BIERRenderTexture.ResizeTexture(LoadTexture("res/BlackPawn.png"), TILE_SIZE, TILE_SIZE) },
             { "WhiteQueen.png", BIERRenderTexture.ResizeTexture(LoadTexture("res/WhiteQueen.png"), TILE_SIZE, TILE_SIZE) },
             { "BlackQueen.png", BIERRenderTexture.ResizeTexture(LoadTexture("res/BlackQueen.png"), TILE_SIZE, TILE_SIZE) },
             { "WhiteRook.png", BIERRenderTexture.ResizeTexture(LoadTexture("res/WhiteRook.png"), TILE_SIZE, TILE_SIZE) },
             { "BlackRook.png", BIERRenderTexture.ResizeTexture(LoadTexture("res/BlackRook.png"), TILE_SIZE, TILE_SIZE) }
        };

        _pieceTextureDict.Where(d => d.Key.Contains("Black")).ToList().ForEach(d =>
        {
            Raylib_CsLo.Image img = Raylib.LoadImageFromTexture(d.Value);

            for (int y = 0; y < img.height; y++)
            {
                for (int x = 0; x < img.width; x++)
                {
                    if (GetImageColor(img, x, y).a > 0)
                    {
                        ImageDrawPixel(&img, x, y, WHITE);
                    }
                }
            }

            Texture selectTexture = LoadTextureFromImage(img);

            UnloadImage(img);

            _pieceTextureDict.Add($"SELECTED{d.Key}", selectTexture);
            
        });

        _romulusFont = LoadFont("res/font_romulus.png");

        IpInput = new
        (
            "",
            BOARDXPos,
            40,
            TILE_SIZE * BOARD_DIMENSIONS,
            50,
            Raylib.WHITE,
            Raylib.BLACK,
            _romulusFont
        );

        UIComponents.Add("HostingText", new BIERButton($"Hosting...         ", 30, 90, 400, 70, BEIGE, GREEN, _romulusFont, 3, false));
        UIComponents.Add("ErrorHostingText", new BIERButton($"ERROR Hosting!     ", 30, 90, 400, 70, BEIGE, RED, _romulusFont, 3, false));
        UIComponents.Add("JoinSuccessText", new BIERButton($"Successfully joined", 30, 150, 400, 70, BEIGE, GREEN, _romulusFont, 3, false));
        UIComponents.Add("ErrorJoinText", new BIERButton($"ERROR Joining!     ", 30, 30, 400, 70, BEIGE, RED, _romulusFont, 3, false));

        Board =
        [
            [new Rook(0, 0, Piece.Color.Black, this), new Knight(0, 1, Piece.Color.Black, this), new Bishop(0, 2, Piece.Color.Black, this), new Queen(0, 3, Piece.Color.Black, this), new King(0, 4, Piece.Color.Black, this), new Bishop(0, 5, Piece.Color.Black, this), new Knight(0, 6, Piece.Color.Black, this), new Rook(0, 7, Piece.Color.Black, this)],
            [new Pawn(1, 0, Piece.Color.Black, this), new Pawn(1, 1, Piece.Color.Black, this), new Pawn(1, 2, Piece.Color.Black, this), new Pawn(1, 3, Piece.Color.Black, this), new Pawn(1, 4, Piece.Color.Black, this), new Pawn(1, 5, Piece.Color.Black, this), new Pawn(1, 6, Piece.Color.Black, this), new Pawn(1, 7, Piece.Color.Black, this)],
            [null,null,null,null,null,null,null,null],
            [null,null,null,null,null,null,null,null],
            [null,null,null,null,null,null,null,null],
            [null,null,null,null,null,null,null,null],
            [new Pawn(6, 0, Piece.Color.White, this), new Pawn(6, 1, Piece.Color.White, this), new Pawn(6, 2, Piece.Color.White, this), new Pawn(6, 3, Piece.Color.White, this), new Pawn(6, 4, Piece.Color.White, this), new Pawn(6, 5, Piece.Color.White, this), new Pawn(6, 6, Piece.Color.White, this), new Pawn(6, 7, Piece.Color.White, this)],
            [new Rook(7, 0, Piece.Color.White, this), new Knight(7, 1, Piece.Color.White, this), new Bishop(7, 2, Piece.Color.White, this), new Queen(7, 3, Piece.Color.White, this), new King(7, 4, Piece.Color.White, this), new Bishop(7, 5, Piece.Color.White, this), new Knight(7, 6, Piece.Color.White, this), new Rook(7, 7, Piece.Color.White, this)]
        ];
        if (!MultiplayerMode)
        {
            ShowMultiplayerMenu();
        }
    }

    public void Gamesettings()
    {
        if (Debugger.IsAttached)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_F1))
            {
                _debugUiHitboxes = !_debugUiHitboxes;
            }
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_F2))
            {
                debugMoveMode = !debugMoveMode;
            }
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_F3))
            {
                Board =
                [
                    [new Rook(0, 0, Piece.Color.White, this), null, null, null, new King(0, 4, Piece.Color.Black, this), null, null, null,],
                    [null,null,null,null,null,null,null,null],
                    [null,null,null,null,null,null,null,null],
                    [null,null,null,null,null,null,null,null],
                    [null,null,null,null,null,null,null,null],
                    [null,null,null,null,null,null,null,null],
                    [null,null,null,null,null,null,null,null],
                    [new Rook(7, 0, Piece.Color.White, this), new Knight(7, 1, Piece.Color.White, this), new Bishop(7, 2, Piece.Color.White, this), new Queen(7, 3, Piece.Color.White, this), new King(7, 4, Piece.Color.White, this), new Bishop(7, 5, Piece.Color.White, this), new Knight(7, 6, Piece.Color.White, this), new Rook(7, 7, Piece.Color.White, this)]
                ];
                debugMoveMode = true;
            }
        }

        if (gameEnds)
        {
            if (restartKeys.Any(key => Raylib.IsKeyPressed(key)))
            {
                continueGame = true;
                gameEnds = false;
                debugMoveMode = false;
                UIComponents.Values.ToList().RemoveAll(c => c is BIERButton || c is BIERInput);
                CloseWindow();
                GameDispose();
                GameInit();
            }
        }
    }
    public void IsGameDone(List<Piece> pieces)
    {
        List<Vector2> WhiteMoves = [.. pieces.Where(p => p.Alignment == Piece.Color.White).SelectMany(p => p.GetMoves())];
        List<Vector2> BlackMoves = [.. pieces.Where(p => p.Alignment == Piece.Color.Black).SelectMany(p => p.GetMoves())];
        if ((WhiteMoves.Count == 0 && BlackMoves.Contains(WhiteKingPosition)) || (BlackMoves.Count == 0 && WhiteMoves.Contains(BlackKingPosition)) && !UIComponents.ContainsKey("SchachmattBtn"))
        {
            UIComponents.Add("SchachmattBtn", new BIERButton(" Schachmatt ", 0, WINDOW_HEIGHT / 2 - WINDOW_HEIGHT / 8f, WINDOW_WIDTH, WINDOW_HEIGHT / 3.5f, BLACK, GOLD, _romulusFont, 3, false));
            gameEnds = true;
            continueGame = false;
            schachmatt = true;
        }
        else if ((WhiteMoves.Count == 0 || BlackMoves.Count == 0) && !UIComponents.ContainsKey("PattBtn") && !schachmatt)
        {
            UIComponents.Add("PattBtn", new BIERButton("   P a t t   ", WINDOW_WIDTH / 2 - WINDOW_WIDTH / 2, WINDOW_HEIGHT / 2 - WINDOW_HEIGHT / 8f, WINDOW_WIDTH, WINDOW_HEIGHT / 3.5f, BLACK, GOLD, _romulusFont, 3, false));
            gameEnds = true;              //" Schachmatt "
            continueGame = false;
        }
    }
    public override void GameUpdate()
    {
        if (MultiplayerManager.IsHostingLive)
            UIComponents["HostingText"].Show();
        else UIComponents["HostingText"].Hide();

        if (MultiplayerManager.IsHostingLiveERROR)
        {
            UIComponents["ErrorHostingText"].Show();
            MultiplayerManager.IsPlayerContected = false;
        }
        else UIComponents["ErrorHostingText"].Hide();

        if (MultiplayerManager.IsPlayerContected)
            UIComponents["JoinSuccessText"].Show();
        else UIComponents["JoinSuccessText"].Hide();

        if (MultiplayerManager.IsPlayerContectedERROR)
            UIComponents["ErrorJoinText"].Show();
        else UIComponents["ErrorJoinText"].Hide();


        Gamesettings();

        if (MultiplayerMode)
        {
            string msg;
            if (MultiplayerManager.TryDequeueMessage(out msg))
            {
                ProcessIncomingMove(msg);
            }
            if (!IsLocalTurn)
                return;
        }

        string localIP = MultiplayerManager.GetLocalIPAddress();
        UIComponents.Remove("YourIpText");
        UIComponents.Add("YourIpText", new BIERButton($"Deine IP: {localIP} FPS: {GetFPS()}", 30, 30, 400, 70, BEIGE, WHITE, _romulusFont, 3, false));

        if (GetActivePieces().All(p => p != null))
            _4ChessMove.MouseUpdate(GetActivePieces(), this);

        if (IpInput.IsVisible)
            HandleKeyTextInput(IpInput);

        IsGameDone(GetActivePieces());
    }

    private void HandleKeyTextInput(BIERInput bierInput)
    {
        int key = GetCharPressed();

        while (key > 0)
        {
            if (key >= 32 && key <= 125)
            {
                bierInput.TextValue += (char)key;
            }

            key = GetCharPressed();
        }

        if (IsKeyPressed(KeyboardKey.KEY_BACKSPACE) && bierInput.TextValue.Length > 0)
        {
            bierInput.TextValue = bierInput.TextValue[..^1];
        }

        bierInput.SyncText();
    }

    private List<Piece> GetActivePieces()
    {
        return [.. Board.SelectMany(row => row)
                                  .Where(piece => piece != null)
                                  .Cast<Piece>()];
    }

    public override void GameRender()
    {
        int renderX = 0;
        int renderY = 0;
        RenderObjects.Clear();

        foreach (var p in Board.SelectMany(row => row))
        {
            if (p != null && p.FilePath != null)
            {
                if (p != _4ChessMove.DraggedPiece)
                {
                    renderX = p.X * TILE_SIZE + BOARDXPos;
                    renderY = p.Y * TILE_SIZE + BOARDYPos;
                    RenderObjects.Add(new BIERRenderTexture(renderX, renderY, TILE_SIZE, TILE_SIZE, color: WHITE)
                    {
                        Texture = _pieceTextureDict[$"{p.FilePath}"]
                    });
                }
            }
        }

        BIERRenderer.Render(RenderObjects, BEIGE, CustomPreRenderFuncs, CustomPostRenderFuncs);
    }


    private void RenderUIComponents()
    {
        if (_debugUiHitboxes)
            UIComponents.SelectMany(c => c.Value.ComponentHitboxes).ToList().ForEach(h =>
            {
                Raylib.DrawRectangle((int)h.x, (int)h.y, (int)h.width, (int)h.height, ColorFromHSV(186, 1f, 0.4f));
            });

        UIComponents.Values.Where(c => c.IsVisible).SelectMany(c => c.ComponentRenderObjects).ToList().ForEach(o => o.Render());
    }

    private void RenderIpInput()
    {
        if (IpInput.IsVisible)
            IpInput.ComponentRenderObjects.ForEach(o => o.Render());
    }

    private void RenderDraggedPiece()
    {
        var draggedPiece = Board.SelectMany(p => p).Where(p => p == _4ChessMove.DraggedPiece).First();
        Vector2 mousePos = Raylib.GetMousePosition();
        int renderX = (int)mousePos.X - TILE_SIZE / 2;
        int renderY = (int)mousePos.Y - TILE_SIZE / 2;
        if (draggedPiece != null && draggedPiece.FilePath != null)
        {
            if (draggedPiece.Alignment == Piece.Color.White)
                Raylib.DrawTextureEx(_pieceTextureDict[draggedPiece.FilePath], new Vector2(renderX, renderY), 0f, 1f, SELECT_COLOR);
            else
                Raylib.DrawTextureEx(_pieceTextureDict[$"SELECTED{draggedPiece.FilePath}"], new Vector2(renderX, renderY), 0f, 1f, SELECT_COLOR);
        }     
    }

    public override void GameDispose()
    {
        RenderObjects.ForEach(o => o.Dispose());
        _pieceTextureDict.Values.ToList().ForEach(t => UnloadTexture(t));
        UnloadFont(_romulusFont);
    }

    private void RenderPossibleMoveRenderTiles()
    {
        _4ChessMove.PossibleMoveRenderTiles.ForEach(r => r.Render());
    }

    private void ShowMultiplayerMenu()
    {
        UIComponents.Add("HostGameBtn", new BIERButton(" Host Game  ", WINDOW_WIDTH / 2 - 200, WINDOW_HEIGHT / 2 - 50, 150, 50, Raylib.WHITE, Raylib.BLACK, null, 2, true)
        {
            ClickEvent = () =>
            {
                MultiplayerMode = true;
                LocalPlayerColor = Piece.Color.White; 
                IsLocalTurn = true;
                IpInput.Hide();
                UIComponents["JoinGameBtn"].Hide();
                UIComponents["HostGameBtn"].Hide();
                MultiplayerManager.IsHost = true;
                System.Threading.Tasks.Task.Run(async () =>
                {
                    await MultiplayerManager.StartHostingAsync();
                });
            }
        });
        UIComponents.Add("JoinGameBtn", new BIERButton(" Join Game  ", WINDOW_WIDTH / 2 + 50, WINDOW_HEIGHT / 2 - 50, 150, 50, Raylib.WHITE, Raylib.BLACK, null, 2, true)
        {
            ClickEvent = () =>
            {
                MultiplayerMode = true;
                LocalPlayerColor = Piece.Color.Black; 
                IsLocalTurn = false;
                IpInput.Hide();
                UIComponents["JoinGameBtn"].Hide();
                UIComponents["HostGameBtn"].Hide();
                string serverIp = IpInput.TextValue.Replace(" ", "").Replace("\n", "");
                MultiplayerManager.IsHost = false;
                System.Threading.Tasks.Task.Run(async () =>
                {
                    await MultiplayerManager.JoinGameAsync(serverIp);
                });
            }
        });
    }



    private void RenderBoard()
    {
        char c = 'w';

        for (int x = 0; x < BOARD_DIMENSIONS; x++)
        {
            for (int y = 0; y < BOARD_DIMENSIONS; y++)
            {
                var color = c switch
                {
                    'w' => LIGHTGRAY,
                    _ => DARKGRAY
                };

                Raylib.DrawRectangle(BOARDXPos + x * TILE_SIZE, BOARDYPos + y * TILE_SIZE, TILE_SIZE, TILE_SIZE, color);

                c = c switch
                {
                    'w' => 'b',
                    _ => 'w'
                };
            }
            c = c switch
            {
                'w' => 'b',
                _ => 'w'
            };
        }
    }

    /// <summary>
    /// Parst eine empfangene Move-Nachricht und aktualisiert das Board.
    /// Erwartetes Format: "MOVE origX origY newX newY"
    /// </summary>
    private void ProcessIncomingMove(string message)
    {
        try
        {
            Board = MoveCounter.DeserializeBoard(message, this);

            IsLocalTurn = true;
            _4ChessMove.TurnChange();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Fehler beim Verarbeiten der Move-Nachricht: " + ex.Message);
        }
    }
}
