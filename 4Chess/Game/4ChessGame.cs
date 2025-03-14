using _4Chess.Game.Input;
using _4Chess.Pieces;
using BIERKELLER.BIERGaming;
using BIERKELLER.BIERRender;
using BIERKELLER.BIERUI;
using Raylib_CsLo;
using System.IO;
using System.Numerics;
using static Raylib_CsLo.Raylib;
using static System.Net.Mime.MediaTypeNames;

namespace _4Chess.Game;

public class _4ChessGame : BIERGame
{
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

    private Raylib_CsLo.Font _romulusFont;
    public List<BIERRenderObject> RenderObjects { get; set; } = [];
    public List<BIERUIComponent> UIComponents { get; set; } = [];
    public List<List<Piece?>> Board { get; set; } = [];

    private Dictionary<string, Texture> _pieceTextureDict = [];
    

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
    }

    public unsafe override void GameInit()
    {
        BIERRenderer.Init(WINDOW_WIDTH, WINDOW_HEIGHT, "4Chess");

        // BIERRender-Objekte erst nach BIERRenderer.Init initialisieren, da sie den GL-Context brauchen!

        _pieceTextureDict = new Dictionary<string, Texture>()
        {
             { "WhiteBishop.png", LoadTexture("res/WhiteBishop.png") },
             { "BlackBishop.png", LoadTexture("res/BlackBishop.png") },
             { "WhiteKing.png", LoadTexture("res/WhiteKing.png") },
             { "BlackKing.png", LoadTexture("res/BlackKing.png") },
             { "WhiteKnight.png", LoadTexture("res/WhiteKnight.png") },
             { "BlackKnight.png", LoadTexture("res/BlackKnight.png") },
             { "WhitePawn.png", LoadTexture("res/WhitePawn.png") },
             { "BlackPawn.png", LoadTexture("res/BlackPawn.png") },
             { "WhiteQueen.png", LoadTexture("res/WhiteQueen.png") },
             { "BlackQueen.png", LoadTexture("res/BlackQueen.png") },
             { "WhiteRook.png", LoadTexture("res/WhiteRook.png") },
             { "BlackRook.png", LoadTexture("res/BlackRook.png") }
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
                        ImageDrawPixel(&img, x, y, SELECT_COLOR);
                    }
                }
            }

            Texture selectTexture = LoadTextureFromImage(img);

            UnloadImage(img);

            _pieceTextureDict.Add($"SELECTED{d.Key}", selectTexture);
        });

        _romulusFont = LoadFont("res/font_romulus.png");

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
    }

    public void Gamesettings()
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
                [null, null, null, null, new King(0, 4, Piece.Color.Black, this), null, null, null,],
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

        if (gameEnds)
        {
            if (restartKeys.Any(key => Raylib.IsKeyPressed(key)))
            {
                continueGame = true;
                gameEnds = false;
                debugMoveMode = false;
                UIComponents.RemoveAll(c => c is BIERButton);
                CloseWindow();
                GameDispose();
                GameInit();
            }
        }
    }
    public void IsGameDone(List<Piece> pieces)
    {
        List<Vector2> WhiteMoves = [.. pieces.Where(p => p.Alignment == Piece.Color.White).SelectMany(p => p.GetMoves(false))];
        List<Vector2> BlackMoves = [.. pieces.Where(p => p.Alignment == Piece.Color.Black).SelectMany(p => p.GetMoves(false))];
        if ((WhiteMoves.Count == 0 && BlackMoves.Contains(WhiteKingPosition)) || (BlackMoves.Count == 0 && WhiteMoves.Contains(BlackKingPosition)))
        {
            UIComponents.Add(new BIERButton(" Schachmatt ", WINDOW_WIDTH / 2 - WINDOW_WIDTH / 2, WINDOW_HEIGHT / 2 - WINDOW_HEIGHT / 8f, WINDOW_WIDTH, WINDOW_HEIGHT / 3.5f, BLACK, GOLD, _romulusFont, 3, false));
            gameEnds = true;
            continueGame = false;
        }
        if (WhiteMoves.Count == 0 || BlackMoves.Count == 0)
        {
            UIComponents.Add(new BIERButton("    Patt ", WINDOW_WIDTH / 2 - WINDOW_WIDTH / 2, WINDOW_HEIGHT / 2 - WINDOW_HEIGHT / 8f, WINDOW_WIDTH, WINDOW_HEIGHT / 3.5f, BLACK, GOLD, _romulusFont, 3, false));
            gameEnds = true;
            continueGame = false;
        }
    }
    public override void GameUpdate()
    {
        //var moveCounter = new MoveCounter();
        //var (totalMoves, uniquePositions) = moveCounter.CountFullMovesAndPositions(this);
        Gamesettings();
        List<Piece> pieces = [.. Board.SelectMany(row => row)
                                  .Where(piece => piece != null)
                                  .Cast<Piece>()];

        if (pieces.All(p => p != null))
            _4ChessMouse.MouseUpdate(pieces, this);

        IsGameDone(pieces);
    }

    public override void GameRender()
    {
        RenderObjects.Clear();
        foreach (var p in Board.SelectMany(row => row))
        {
            if (p != null && p.FilePath != null)
            {
                int renderX, renderY;
                if (p != _4ChessMouse.DraggedPiece)
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
            UIComponents.SelectMany(c => c.CompnentHitboxes).ToList().ForEach(h =>
            {
                Raylib.DrawRectangle((int)h.x, (int)h.y, (int)h.width, (int)h.height, ColorFromHSV(186, 1f, 0.4f));
            });

        UIComponents.Where(c => c.IsVisible).SelectMany(c => c.ComponentRenderObjects).ToList().ForEach(o => o.Render());
    }

    private void RenderDraggedPiece()
    {
        var draggedPiece = Board.SelectMany(p => p).Where(p => p == _4ChessMouse.DraggedPiece).First();
        Vector2 mousePos = Raylib.GetMousePosition();
        int renderX = (int)mousePos.X - TILE_SIZE / 2;
        int renderY = (int)mousePos.Y - TILE_SIZE / 2;
        if (draggedPiece != null && draggedPiece.FilePath != null)
        {
            if (draggedPiece.Alignment == Piece.Color.White)
                Raylib.DrawTextureEx(_pieceTextureDict[draggedPiece.FilePath], new Vector2(renderX, renderY), 0f, 1f, SELECT_COLOR);
            else
                Raylib.DrawTextureEx(_pieceTextureDict[$"SELECTED{draggedPiece.FilePath}"], new Vector2(renderX, renderY), 0f, 1f, WHITE);
        }
            
    }

    public override void GameDispose()
    {
        RenderObjects.ForEach(o => o.Dispose());
        _pieceTextureDict.Values.ToList().ForEach(t => UnloadTexture(t));
    }

    private void RenderPossibleMoveRenderTiles()
    {
        _4ChessMouse.PossibleMoveRenderTiles.ForEach(r => r.Render());
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
}
