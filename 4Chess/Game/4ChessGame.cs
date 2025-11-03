using _4Chess.ChessAi;
using _4Chess.Game.Input;
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
    public const int WINDOW_HEIGHT = 1080;
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

    private bool _isLoadingAi = false;
    private string _loadingMessage = "Loading AI...";

    private Raylib_CsLo.Font _romulusFont;
    public List<BIERRenderObject> RenderObjects { get; set; } = [];
    public Dictionary<string, BIERUIComponent> UIComponents { get; set; } = [];
    public List<List<Piece?>> Board { get; set; } = [];
    public static readonly object BoardLock = new object(); // Thread-Synchronisation für Board-Zugriff

    public static ChessAi.ChessAi ChessAi { get; set; } = null!;
    public static StockfishAi StockfishAi { get; set; } = null!;
    public static CustomAi CustomAi { get; set; } = null!;


    private Dictionary<string, Texture> _pieceTextureDict = [];

    public static bool MultiplayerMode = false;
    public static Piece.Color LocalPlayerColor = Piece.Color.White;
    public static bool IsLocalTurn { get; set; } = true;
    public static bool AiMode { get; set; } = false;
    private static bool isAiThinking = false;
    public bool IsAiThinking => isAiThinking;

    // AI Tournament
    public AiTournament? CurrentTournament { get; set; } = null;
    public bool TournamentMode { get; set; } = false;
    private ChessAiDifficulty _tournamentWhiteAi = ChessAiDifficulty.Medium;
    private ChessAiDifficulty _tournamentBlackAi = ChessAiDifficulty.High;
    private int _tournamentGameCount = 10;


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
        CustomPostRenderFuncs.Add(RenderStockfishThinking);
        CustomPostRenderFuncs.Add(RenderTournamentResults);
        CustomPostRenderFuncs.Add(RenderLoadingIndicator); // NEU
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
        List<Vector2> WhiteMoves;
        List<Vector2> BlackMoves;

        // Lock Board während wir alle Moves berechnen
        lock (BoardLock)
        {
            WhiteMoves = [.. pieces.Where(p => p.Alignment == Piece.Color.White).SelectMany(p => p.GetMoves())];
            BlackMoves = [.. pieces.Where(p => p.Alignment == Piece.Color.Black).SelectMany(p => p.GetMoves())];
        }
        // Schachmatt: Entweder Weiß hat keine Züge und steht im Schach ODER Schwarz hat keine Züge und steht im Schach.
        // Button nur hinzufügen, wenn er noch nicht existiert.
        if (((WhiteMoves.Count == 0 && BlackMoves.Contains(WhiteKingPosition))
             || (BlackMoves.Count == 0 && WhiteMoves.Contains(BlackKingPosition)))
            && !UIComponents.ContainsKey("SchachmattBtn"))
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

        // AI Logic
        if (AiMode && !isAiThinking && continueGame)
        {
            _4Chess.Logger.LogInfo($"AI Check: AiMode={AiMode}, isAiThinking={isAiThinking}, IsWhiteTurn={_4ChessMove.IsWhiteTurn}");
            // Check if it's AI's turn (Black)
            if (!_4ChessMove.IsWhiteTurn)
            {
                _4Chess.Logger.LogInfo("AI starting move...");
                isAiThinking = true;
                // Capture current state for async task
                bool currentTurn = _4ChessMove.IsWhiteTurn;
                int currentMoveCount = _4ChessMove.MoveCounter;

                Task.Run(async () =>
                {
                    try
                    {
                        _4Chess.Logger.LogInfo($"AI Task started - currentTurn={currentTurn}, moveCount={currentMoveCount}");
                        if (StockfishAi != null)
                        {
                            await StockfishAi.MakeMoveAsync(this, currentTurn, currentMoveCount);
                        }
                        else if (CustomAi != null)
                        {
                            // CustomAi logic here
                            _4Chess.Logger.LogWarning("CustomAi not yet fully implemented");
                        }
                        _4Chess.Logger.LogInfo("AI Task completed successfully");
                    }
                    catch (Exception ex)
                    {
                        _4Chess.Logger.LogError("AI Error", ex);
                    }
                    finally
                    {
                        _4Chess.Logger.LogInfo("AI Task finally block - setting isAiThinking to false");
                        isAiThinking = false;
                    }
                });
            }
        }

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

        if (IpInput.IsVisible)
            HandleKeyTextInput(IpInput);

        // Skip all board-related updates while AI is thinking to avoid race conditions
        if (!isAiThinking)
        {
            if (GetActivePieces().All(p => p != null))
                _4ChessMove.MouseUpdate(GetActivePieces(), this);

            IsGameDone(GetActivePieces());
        }
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
        lock (BoardLock)
        {
            return [.. Board.SelectMany(row => row)
                                      .Where(piece => piece != null)
                                      .Cast<Piece>()];
        }
    }

    public override void GameRender()
    {
        int renderX = 0;
        int renderY = 0;
        RenderObjects.Clear();

        // Lock board access during rendering to prevent race conditions with AI thread
        // Materialize the collection inside the lock to avoid deferred execution issues
        List<Piece?> pieces;
        lock (BoardLock)
        {
            pieces = Board.SelectMany(row => row).ToList();
        }

        foreach (var p in pieces)
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
        // Materialize the collection inside the lock to avoid deferred execution issues
        Piece? draggedPiece;
        lock (BoardLock)
        {
            // First materialize with ToList(), then search
            draggedPiece = Board.SelectMany(p => p).ToList().Where(p => p == _4ChessMove.DraggedPiece).FirstOrDefault();
        }

        if (draggedPiece != null && draggedPiece.FilePath != null)
        {
            Vector2 mousePos = Raylib.GetMousePosition();
            int renderX = (int)mousePos.X - TILE_SIZE / 2;
            int renderY = (int)mousePos.Y - TILE_SIZE / 2;

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

        // Dispose AI resources
        if (StockfishAi != null)
        {
            StockfishAi.Dispose();
        }
    }

    private void RenderPossibleMoveRenderTiles()
    {
        _4ChessMove.PossibleMoveRenderTiles.ForEach(r => r.Render());
    }

    private void ShowMultiplayerMenu()
    {
        UIComponents.Add("PlayAiBtn", new BIERButton(" Play against Ai ", x: WINDOW_WIDTH / 2 - 450, y: WINDOW_HEIGHT / 2 - 50, w: 200, h: 100, Raylib.WHITE, Raylib.BLACK, null, 2, true)
        {
            ClickEvent = () =>
            {
                MultiplayerMode = true;
                LocalPlayerColor = Piece.Color.White;
                IsLocalTurn = true;
                IpInput.Hide();
                UIComponents["PlayAiBtn"].Hide();
                UIComponents["JoinGameBtn"].Hide();
                UIComponents["HostGameBtn"].Hide();

                //StockFish Button
                UIComponents.Add("PlayStockfishBtn", new BIERButton(" Play against Stockfish ", x: WINDOW_WIDTH / 2 - 450, y: WINDOW_HEIGHT / 2 - 50, w: 200, h: 100, Raylib.WHITE, Raylib.BLACK, null, 2, true)
                {
                    ClickEvent = () =>
                    {
                        UIComponents["PlayStockfishBtn"].Hide();
                        UIComponents["PlayCustomBtn"].Hide();
                        UIComponents["AiTournamentBtn"].Hide();

                        // Difficulty selection buttons - Row 1
                        UIComponents.Add("DifficultyLowBtn", new BIERButton(" Easy ", WINDOW_WIDTH / 2 - 470, WINDOW_HEIGHT / 2 - 120, 220, 70, WHITE, GREEN, null, 2, true)
                        {
                            ClickEvent = () =>
                            {
                                InitializeAiAsync(1, 0, "Loading Easy AI...");
                                HideDifficultyButtons();
                                ShowLoadingIndicator();
                                Task.Run(() =>
                                {
                                    ChessAi = new ChessAi.ChessAi(1, 0); // Stockfish Low
                                    AiMode = true;
                                    HideLoadingIndicator();
                                });
                            }
                        });

                        UIComponents.Add("DifficultyMediumBtn", new BIERButton(" Medium ", WINDOW_WIDTH / 2 - 230, WINDOW_HEIGHT / 2 - 120, 220, 70, WHITE, YELLOW, null, 2, true)
                        {
                            ClickEvent = () =>
                            {
                                InitializeAiAsync(1, 1, "Loading Medium AI...");
                                HideDifficultyButtons();
                                ShowLoadingIndicator();
                                Task.Run(() =>
                                {
                                    ChessAi = new ChessAi.ChessAi(1, 1); // Stockfish Medium
                                    AiMode = true;
                                    HideLoadingIndicator();
                                });
                            }
                        });

                        UIComponents.Add("DifficultyHighBtn", new BIERButton(" Hard ", WINDOW_WIDTH / 2 + 10, WINDOW_HEIGHT / 2 - 120, 220, 70, WHITE, ORANGE, null, 2, true)
                        {
                            ClickEvent = () =>
                            {
                                InitializeAiAsync(1, 2, "Loading Hard AI...");
                                HideDifficultyButtons();
                                ShowLoadingIndicator();
                                Task.Run(() =>
                                {
                                    ChessAi = new ChessAi.ChessAi(1, 2); // Stockfish High
                                    AiMode = true;
                                    HideLoadingIndicator();
                                });
                            }
                        });

                        UIComponents.Add("DifficultyExpertBtn", new BIERButton(" Expert ", WINDOW_WIDTH / 2 + 250, WINDOW_HEIGHT / 2 - 120, 220, 70, WHITE, RED, null, 2, true)
                        {
                            ClickEvent = () =>
                            {
                                InitializeAiAsync(1, 3, "Loading Expert AI...");
                                HideDifficultyButtons();
                                ShowLoadingIndicator();
                                Task.Run(() =>
                                {
                                    ChessAi = new ChessAi.ChessAi(1, 3); // Stockfish Expert
                                    AiMode = true;
                                    HideLoadingIndicator();
                                });
                            }
                        });

                        // Difficulty selection buttons - Row 2 (Ultra modes)
                        UIComponents.Add("DifficultyUltraBtn", new BIERButton(" Ultra ", WINDOW_WIDTH / 2 - 470, WINDOW_HEIGHT / 2 - 30, 220, 70, WHITE, DARKPURPLE, null, 2, true)
                        {
                            ClickEvent = () =>
                            {
                                InitializeAiAsync(1, 4, "Loading Ultra AI...");
                                HideDifficultyButtons();
                                ShowLoadingIndicator();
                                Task.Run(() =>
                                {
                                    ChessAi = new ChessAi.ChessAi(1, 4); // Stockfish Ultra
                                    AiMode = true;
                                    HideLoadingIndicator();
                                });
                            }
                        });

                        UIComponents.Add("DifficultyUltraPlusBtn", new BIERButton(" Ultra+ ", WINDOW_WIDTH / 2 - 230, WINDOW_HEIGHT / 2 - 30, 220, 70, WHITE, VIOLET, null, 2, true)
                        {
                            ClickEvent = () =>
                            {
                                InitializeAiAsync(1, 5, "Loading Ultra+ AI...");
                                HideDifficultyButtons();
                                ShowLoadingIndicator();
                                Task.Run(() =>
                                {
                                    ChessAi = new ChessAi.ChessAi(1, 5); // Stockfish UltraPlus
                                    AiMode = true;
                                    HideLoadingIndicator();
                                });
                            }
                        });

                        UIComponents.Add("DifficultyGodlikeBtn", new BIERButton(" Godlike ", WINDOW_WIDTH / 2 + 10, WINDOW_HEIGHT / 2 - 30, 220, 70, WHITE, PINK, null, 2, true)
                        {
                            ClickEvent = () =>
                            {
                                InitializeAiAsync(1, 6, "Loading Godlike AI...");
                                HideDifficultyButtons();
                                ShowLoadingIndicator();
                                Task.Run(() =>
                                {
                                    ChessAi = new ChessAi.ChessAi(1, 6); // Stockfish Godlike
                                    AiMode = true;
                                    HideLoadingIndicator();
                                });
                            }
                        });

                        UIComponents.Add("DifficultyOverthinkerBtn", new BIERButton(" You will Lose ", WINDOW_WIDTH / 2 + 250, WINDOW_HEIGHT / 2 - 30, 220, 70, WHITE, MAGENTA, null, 2, true)
                        {
                            ClickEvent = () =>
                            {
                                InitializeAiAsync(1, 7, "Loading Ultimate AI...");
                                HideDifficultyButtons();
                                ShowLoadingIndicator();
                                Task.Run(() =>
                                {
                                    ChessAi = new ChessAi.ChessAi(1, 7); // Stockfish Overthinker
                                    AiMode = true;
                                    HideLoadingIndicator();
                                });
                            }
                        });
                    }
                });
                //CustomAi Button
                UIComponents.Add("PlayCustomBtn", new BIERButton(" Play against CustomAi  ", x: WINDOW_WIDTH / 2 + 250, y: WINDOW_HEIGHT / 2 - 50, w: 200, h: 100, Raylib.WHITE, Raylib.BLACK, null, 2, true)
                {
                    ClickEvent = () =>
                    {
                        UIComponents["PlayStockfishBtn"].Hide();
                        UIComponents["PlayCustomBtn"].Hide();
                        UIComponents["AiTournamentBtn"].Hide();
                        ShowLoadingIndicator();
                        Task.Run(() =>
                        {
                            ChessAi = new ChessAi.ChessAi(2); // 2 = CustomAi
                            AiMode = true;
                            HideLoadingIndicator();
                        });
                    }
                });

                //AI Tournament Button
                UIComponents.Add("AiTournamentBtn", new BIERButton(" AI Tournament ", x: WINDOW_WIDTH / 2 - 100, y: WINDOW_HEIGHT / 2 + 70, w: 200, h: 100, Raylib.WHITE, Raylib.PURPLE, null, 2, true)
                {
                    ClickEvent = () =>
                    {
                        UIComponents["PlayStockfishBtn"].Hide();
                        UIComponents["PlayCustomBtn"].Hide();
                        UIComponents["AiTournamentBtn"].Hide();
                        ShowTournamentSetup();
                    }
                });
            }
        });
        UIComponents.Add("HostGameBtn", new BIERButton(" Host Game  ", x: WINDOW_WIDTH / 2 - 100, y: WINDOW_HEIGHT / 2 - 50, w: 200, h: 100, Raylib.WHITE, Raylib.BLACK, null, 2, true)
        {
            ClickEvent = () =>
            {
                MultiplayerMode = true;
                LocalPlayerColor = Piece.Color.White;
                IsLocalTurn = true;
                IpInput.Hide();
                UIComponents["PlayAiBtn"].Hide();
                UIComponents["JoinGameBtn"].Hide();
                UIComponents["HostGameBtn"].Hide();
                HideAiSelectionButtons();
                MultiplayerManager.IsHost = true;
                System.Threading.Tasks.Task.Run(async () =>
                {
                    await MultiplayerManager.StartHostingAsync();
                });
            }
        });
        UIComponents.Add("JoinGameBtn", new BIERButton(" Join Game  ", x: WINDOW_WIDTH / 2 + 250, y: WINDOW_HEIGHT / 2 - 50, w: 200, h: 100, Raylib.WHITE, Raylib.BLACK, null, 2, true)
        {
            ClickEvent = () =>
            {
                MultiplayerMode = true;
                LocalPlayerColor = Piece.Color.Black;
                IsLocalTurn = false;
                IpInput.Hide();
                UIComponents["PlayAiBtn"].Hide();
                UIComponents["JoinGameBtn"].Hide();
                UIComponents["HostGameBtn"].Hide();
                HideAiSelectionButtons();
                string serverIp = IpInput.TextValue.Replace(" ", "").Replace("\n", "");
                MultiplayerManager.IsHost = false;
                System.Threading.Tasks.Task.Run(async () =>
                {
                    await MultiplayerManager.JoinGameAsync(serverIp);
                });
            }
        });
    }

    private async void InitializeAiAsync(int aiType, int difficulty, string loadingMessage)
    {
        _loadingMessage = loadingMessage;
        _isLoadingAi = true;
        HideDifficultyButtons();

        await Task.Run(() =>
        {
            try
            {
                Logger.LogInfo($"Initializing AI: Type={aiType}, Difficulty={difficulty}");
                ChessAi = new ChessAi.ChessAi(aiType, difficulty);
                Logger.LogInfo("AI initialization completed");
            }
            catch (Exception ex)
            {
                Logger.LogError("AI initialization failed", ex);
            }
        });

        // Kleine Verzögerung für besseres UX
        await Task.Delay(500);

        _isLoadingAi = false;
        AiMode = true;
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

    private void HideDifficultyButtons()
    {
        if (UIComponents.ContainsKey("DifficultyLowBtn")) UIComponents["DifficultyLowBtn"].Hide();
        if (UIComponents.ContainsKey("DifficultyMediumBtn")) UIComponents["DifficultyMediumBtn"].Hide();
        if (UIComponents.ContainsKey("DifficultyHighBtn")) UIComponents["DifficultyHighBtn"].Hide();
        if (UIComponents.ContainsKey("DifficultyExpertBtn")) UIComponents["DifficultyExpertBtn"].Hide();
        if (UIComponents.ContainsKey("DifficultyUltraBtn")) UIComponents["DifficultyUltraBtn"].Hide();
        if (UIComponents.ContainsKey("DifficultyUltraPlusBtn")) UIComponents["DifficultyUltraPlusBtn"].Hide();
        if (UIComponents.ContainsKey("DifficultyGodlikeBtn")) UIComponents["DifficultyGodlikeBtn"].Hide();
        if (UIComponents.ContainsKey("DifficultyOverthinkerBtn")) UIComponents["DifficultyOverthinkerBtn"].Hide();
    }

    private void HideAiSelectionButtons()
    {
        if (UIComponents.ContainsKey("PlayStockfishBtn")) UIComponents["PlayStockfishBtn"].Hide();
        if (UIComponents.ContainsKey("PlayCustomBtn")) UIComponents["PlayCustomBtn"].Hide();
        if (UIComponents.ContainsKey("AiTournamentBtn")) UIComponents["AiTournamentBtn"].Hide();
    }

    private void RenderLoadingIndicator()
    {
        if (!_isLoadingAi)
            return;

        // Semi-transparenter Hintergrund über dem gesamten Bildschirm
        DrawRectangle(0, 0, WINDOW_WIDTH, WINDOW_HEIGHT, ColorAlpha(BLACK, 0.7f));

        // Loading-Box
        int boxWidth = 600;
        int boxHeight = 200;
        int boxX = WINDOW_WIDTH / 2 - boxWidth / 2;
        int boxY = WINDOW_HEIGHT / 2 - boxHeight / 2;

        DrawRectangle(boxX, boxY, boxWidth, boxHeight, ColorAlpha(DARKGRAY, 0.95f));
        DrawRectangleLines(boxX, boxY, boxWidth, boxHeight, GOLD);

        // Titel
        DrawTextEx(_romulusFont, _loadingMessage,
            new Vector2(boxX + boxWidth / 2 - MeasureTextEx(_romulusFont, _loadingMessage, 40, 3).X / 2, boxY + 30),
            40, 3, GOLD);

        // Animierter Ladebalken
        int barWidth = boxWidth - 100;
        int barHeight = 30;
        int barX = boxX + 50;
        int barY = boxY + 100;

        DrawRectangle(barX, barY, barWidth, barHeight, DARKGRAY);

        // Animierte Füllung basierend auf Zeit
        float progress = (float)(GetTime() % 2.0) / 2.0f; // 2 Sekunden Zyklus
        int fillWidth = (int)(barWidth * progress);
        DrawRectangle(barX, barY, fillWidth, barHeight, SKYBLUE);

        DrawRectangleLines(barX, barY, barWidth, barHeight, WHITE);

        // Fortschrittstext
        string statusText = "Please wait...";
        DrawTextEx(_romulusFont, statusText,
            new Vector2(boxX + boxWidth / 2 - MeasureTextEx(_romulusFont, statusText, 25, 2).X / 2, boxY + 150),
            25, 2, WHITE);
    }

    private void RenderStockfishThinking()
    {
        if (StockfishAi == null || !StockfishAi.ShowThinking)
            return;

        // Erstelle eine Thread-sichere Kopie der ThinkingLines
        List<ChessAi.StockfishThinkingLine> thinkingLinesCopy;
        lock (StockfishAi.ThinkingLines)
        {
            if (StockfishAi.ThinkingLines.Count == 0)
                return;

            thinkingLinesCopy = new List<ChessAi.StockfishThinkingLine>(StockfishAi.ThinkingLines);
        }

        int startX = BOARDXPos + (BOARD_DIMENSIONS * TILE_SIZE) + 50;
        int startY = BOARDYPos;
        int lineHeight = 40;
        int boxWidth = 600;
        int headerHeight = 130; // Erhöht für Depth-Anzeige und Timer

        // Hintergrund
        DrawRectangle(startX - 10, startY - 10, boxWidth, headerHeight + (thinkingLinesCopy.Count * lineHeight), ColorAlpha(BLACK, 0.8f));

        // Titel
        DrawTextEx(_romulusFont, "Ai Thinking:", new Vector2(startX, startY), 30, 2, GOLD);

        // Depth-Anzeige
        string depthText = $"Depth: {StockfishAi.CurrentDepth}/{StockfishAi.TargetDepth}";
        Color depthColor = StockfishAi.CurrentDepth >= StockfishAi.TargetDepth ? GREEN : SKYBLUE;
        DrawTextEx(_romulusFont, depthText, new Vector2(startX, startY + 35), 25, 2, depthColor);

        // Timer-Anzeige
        if (StockfishAi.IsThinking)
        {
            var elapsed = DateTime.Now - StockfishAi.ThinkingStartTime;
            var remaining = StockfishAi.CurrentMoveTimeMs - (int)elapsed.TotalMilliseconds;
            remaining = Math.Max(0, remaining); // Verhindere negative Werte

            string timerText = $"Time: {elapsed.TotalSeconds:F1}s / {(StockfishAi.CurrentMoveTimeMs / 1000.0):F1}s (Remaining: {(remaining / 1000.0):F1}s)";
            Color timerColor = remaining < 3000 ? RED : (remaining < 5000 ? ORANGE : WHITE);
            DrawTextEx(_romulusFont, timerText, new Vector2(startX, startY + 65), 20, 1, timerColor);
        }
        else
        {
            string timerText = "Time: Finished";
            DrawTextEx(_romulusFont, timerText, new Vector2(startX, startY + 65), 20, 1, GREEN);
        }

        int currentY = startY + headerHeight;
        // Mate-Info (falls vorhanden)
        var mateLine = thinkingLinesCopy.FirstOrDefault(l => l.ScoreType == "mate");
        if (mateLine != null)
        {
            string mateText = $"Mate: {mateLine.GetScoreDisplay()}";
            Color mateColor = mateLine.Score > 0 ? GREEN : RED;
            DrawTextEx(_romulusFont, mateText, new Vector2(startX, startY + 90), 20, 1, mateColor);
        }

        // Zeige alle Thinking Lines
        foreach (var line in thinkingLinesCopy)
        {
            string lineText = $"#{line.MultiPVIndex} [{line.Depth}] {line.GetScoreDisplay()}: {line.GetMovesDisplay(5)}";

            // Färbe die beste Line grün, schlechtere rot
            Color lineColor = line.MultiPVIndex switch
            {
                1 => GREEN,
                2 => YELLOW,
                3 => ORANGE,
                _ => RED
            };

            DrawTextEx(_romulusFont, lineText, new Vector2(startX, currentY), 20, 1, lineColor);
            currentY += lineHeight;
        }

        // Status-Text
        string status = isAiThinking ? "AI is thinking..." : "AI ready";
        DrawTextEx(_romulusFont, status, new Vector2(startX, currentY + 20), 24, 1, isAiThinking ? YELLOW : GREEN);
    }

    private void ShowTournamentSetup()
    {
        // White AI selection
        int y = WINDOW_HEIGHT / 2 - 200;
        for (int i = 0; i <= 7; i++)
        {
            ChessAiDifficulty diff = (ChessAiDifficulty)i;
            string btnName = $"TournamentWhite{diff}Btn";
            UIComponents.Add(btnName, new BIERButton($" {diff} ", WINDOW_WIDTH / 2 - 450, y, 180, 50, WHITE, SKYBLUE, null, 2, true)
            {
                ClickEvent = () =>
                {
                    _tournamentWhiteAi = diff;
                    _4Chess.Logger.LogInfo($"White AI set to {diff}");
                }
            });
            y += 60;
        }

        // Black AI selection
        y = WINDOW_HEIGHT / 2 - 200;
        for (int i = 0; i <= 7; i++)
        {
            ChessAiDifficulty diff = (ChessAiDifficulty)i;
            string btnName = $"TournamentBlack{diff}Btn";
            UIComponents.Add(btnName, new BIERButton($" {diff} ", WINDOW_WIDTH / 2 + 270, y, 180, 50, WHITE, ORANGE, null, 2, true)
            {
                ClickEvent = () =>
                {
                    _tournamentBlackAi = diff;
                    _4Chess.Logger.LogInfo($"Black AI set to {diff}");
                }
            });
            y += 60;
        }

        // Game count buttons
        int[] gameCounts = [5, 10, 25, 50, 100];
        int x = WINDOW_WIDTH / 2 - 250;
        foreach (int count in gameCounts)
        {
            UIComponents.Add($"TournamentGames{count}Btn", new BIERButton($" {count} ", x, WINDOW_HEIGHT / 2 + 150, 100, 50, WHITE, GREEN, null, 2, true)
            {
                ClickEvent = () =>
                {
                    _tournamentGameCount = count;
                    _4Chess.Logger.LogInfo($"Game count set to {count}");
                }
            });
            x += 110;
        }

        // Start Tournament Button
        UIComponents.Add("StartTournamentBtn", new BIERButton(" START TOURNAMENT ", WINDOW_WIDTH / 2 - 150, WINDOW_HEIGHT / 2 + 220, 300, 80, WHITE, RED, null, 2, true)
        {
            ClickEvent = () =>
            {
                StartTournament();
            }
        });

        // Labels
        UIComponents.Add("TournamentWhiteLabel", new BIERLabel(" White AI: ", WINDOW_WIDTH / 2 - 450, WINDOW_HEIGHT / 2 - 250, 30, WHITE));
        UIComponents.Add("TournamentBlackLabel", new BIERLabel(" Black AI: ", WINDOW_WIDTH / 2 + 270, WINDOW_HEIGHT / 2 - 250, 30, WHITE));
        UIComponents.Add("TournamentGamesLabel", new BIERLabel(" Number of Games: ", WINDOW_WIDTH / 2 - 250, WINDOW_HEIGHT / 2 + 110, 25, WHITE));
    }

    private void HideTournamentSetup()
    {
        for (int i = 0; i <= 7; i++)
        {
            ChessAiDifficulty diff = (ChessAiDifficulty)i;
            if (UIComponents.ContainsKey($"TournamentWhite{diff}Btn")) UIComponents[$"TournamentWhite{diff}Btn"].Hide();
            if (UIComponents.ContainsKey($"TournamentBlack{diff}Btn")) UIComponents[$"TournamentBlack{diff}Btn"].Hide();
        }

        int[] gameCounts = [5, 10, 25, 50, 100];
        foreach (int count in gameCounts)
        {
            if (UIComponents.ContainsKey($"TournamentGames{count}Btn")) UIComponents[$"TournamentGames{count}Btn"].Hide();
        }

        if (UIComponents.ContainsKey("StartTournamentBtn")) UIComponents["StartTournamentBtn"].Hide();
        if (UIComponents.ContainsKey("TournamentWhiteLabel")) UIComponents["TournamentWhiteLabel"].Hide();
        if (UIComponents.ContainsKey("TournamentBlackLabel")) UIComponents["TournamentBlackLabel"].Hide();
        if (UIComponents.ContainsKey("TournamentGamesLabel")) UIComponents["TournamentGamesLabel"].Hide();
    }

    private void StartTournament()
    {
        HideTournamentSetup();
        TournamentMode = true;
        CurrentTournament = new AiTournament(_tournamentWhiteAi, _tournamentBlackAi, _tournamentGameCount);

        _4Chess.Logger.LogInfo($"Starting tournament: {_tournamentWhiteAi} vs {_tournamentBlackAi}, {_tournamentGameCount} games");

        // Set AI thinking flag for the entire tournament duration
        isAiThinking = true;

        // Start tournament in background
        Task.Run(async () =>
        {
            try
            {
                await CurrentTournament.RunTournamentAsync(this);
            }
            finally
            {
                // Reset AI thinking flag when tournament is done
                isAiThinking = false;
            }
        });
    }

    private void RenderTournamentResults()
    {
        if (CurrentTournament == null || !TournamentMode)
            return;

        int startX = WINDOW_WIDTH / 2 - 400;
        int startY = WINDOW_HEIGHT / 2 - 300;
        int boxWidth = 800;
        int boxHeight = 600;

        // Background
        DrawRectangle(startX - 10, startY - 10, boxWidth, boxHeight, ColorAlpha(BLACK, 0.9f));

        // Title
        DrawTextEx(_romulusFont, "AI TOURNAMENT", new Vector2(startX + boxWidth / 2 - 150, startY), 40, 3, GOLD);

        // Matchup
        string matchup = $"{CurrentTournament.Result.WhiteAiName} vs {CurrentTournament.Result.BlackAiName}";
        DrawTextEx(_romulusFont, matchup, new Vector2(startX + 50, startY + 60), 25, 2, WHITE);

        // Progress
        string progress = $"Game: {CurrentTournament.Result.CurrentGame} / {CurrentTournament.Result.TotalGames}";
        DrawTextEx(_romulusFont, progress, new Vector2(startX + 50, startY + 100), 25, 2, SKYBLUE);

        // Scores
        int scoreY = startY + 150;
        DrawTextEx(_romulusFont, "RESULTS:", new Vector2(startX + 50, scoreY), 30, 2, GOLD);

        scoreY += 50;
        string whiteScore = $"White Wins: {CurrentTournament.Result.WhiteWins}";
        DrawTextEx(_romulusFont, whiteScore, new Vector2(startX + 50, scoreY), 25, 2, GREEN);

        scoreY += 40;
        string blackScore = $"Black Wins: {CurrentTournament.Result.BlackWins}";
        DrawTextEx(_romulusFont, blackScore, new Vector2(startX + 50, scoreY), 25, 2, RED);

        scoreY += 40;
        string drawScore = $"Draws: {CurrentTournament.Result.Draws}";
        DrawTextEx(_romulusFont, drawScore, new Vector2(startX + 50, scoreY), 25, 2, YELLOW);

        // Win percentage
        if (CurrentTournament.Result.CurrentGame > 0)
        {
            scoreY += 60;
            float whitePercent = (float)CurrentTournament.Result.WhiteWins / CurrentTournament.Result.CurrentGame * 100f;
            float blackPercent = (float)CurrentTournament.Result.BlackWins / CurrentTournament.Result.CurrentGame * 100f;

            DrawTextEx(_romulusFont, $"White: {whitePercent:F1}%", new Vector2(startX + 50, scoreY), 22, 2, SKYBLUE);
            DrawTextEx(_romulusFont, $"Black: {blackPercent:F1}%", new Vector2(startX + 50, scoreY + 30), 22, 2, ORANGE);
        }

        // Status
        scoreY += 80;
        string status = CurrentTournament.Result.IsFinished ? "TOURNAMENT FINISHED!" : "Tournament in progress...";
        Color statusColor = CurrentTournament.Result.IsFinished ? GREEN : YELLOW;
        DrawTextEx(_romulusFont, status, new Vector2(startX + 50, scoreY), 28, 2, statusColor);

        // Exit button if finished
        if (CurrentTournament.Result.IsFinished)
        {
            if (!UIComponents.ContainsKey("ExitTournamentBtn"))
            {
                UIComponents.Add("ExitTournamentBtn", new BIERButton(" BACK TO MENU ", WINDOW_WIDTH / 2 - 150, startY + boxHeight - 80, 300, 60, WHITE, RED, null, 2, true)
                {
                    ClickEvent = () =>
                    {
                        TournamentMode = false;
                        CurrentTournament = null;
                        UIComponents["ExitTournamentBtn"].Hide();
                        GameInit(); // Reset game
                    }
                });
            }
        }
    }

    private void ShowLoadingIndicator()
    {
        if (!UIComponents.ContainsKey("LoadingIndicator"))
        {
            UIComponents.Add("LoadingIndicator", new BIERButton(" Loading AI... ", WINDOW_WIDTH / 2 - 200, WINDOW_HEIGHT / 2 - 50, 400, 100, BLACK, YELLOW, _romulusFont, 3, false));
        }
        UIComponents["LoadingIndicator"].Show();
    }

    private void HideLoadingIndicator()
    {
        if (UIComponents.ContainsKey("LoadingIndicator"))
        {
            UIComponents["LoadingIndicator"].Hide();
        }
    }
}
