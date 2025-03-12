using _4Chess.Pieces;
using BIERKELLER.BIERGaming;
using BIERKELLER.BIERInputs;
using BIERKELLER.BIERRender;
using Raylib_CsLo;
using System.IO;
using System.Numerics;
using static Raylib_CsLo.Raylib;

namespace _4Chess.Game;

public class _4ChessGame : BIERGame
{
    public const int WINDOW_WIDTH = 1920;
    public const int WINDOW_HEIGHT = 1200;
    public const int BOARD_DIMENSIONS = 8;
    public static readonly int BOARDXPos = WINDOW_WIDTH / 4;
    public static readonly int BOARDYPos = WINDOW_HEIGHT / 8;
    public static readonly int TILE_SIZE = (WINDOW_WIDTH - (BOARDXPos * 2)) / BOARD_DIMENSIONS;

    public List<BIERRenderObject> _renderObjects = [];
    public List<List<Piece?>> Board { get; set; } = [];

    private Dictionary<string, Texture> _pieceTextureDict = [];

    public Vector2 WhiteKingPosition { get; set; }
    public Vector2 BlackKingPosition { get; set; }

    public _4ChessGame()
    {
        CustomPreRenderFuncs.Add(RenderBoard);
    }

    public override void GameInit()
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

        Board =
        [
            [new Pawn(0, 0, Piece.Color.Black, this), new Pawn(1, 0, Piece.Color.Black, this), new Pawn(0, 1, Piece.Color.White, this), new Rook(0, 2, Piece.Color.White, this), new Pawn(0, 0, Piece.Color.Black, this), new Queen(7, 6, Piece.Color.Black, this), new Pawn(0, 0, Piece.Color.Black, this), new Pawn(0, 0, Piece.Color.Black, this)]
        ];
    }

    public override void GameUpdate()
    {
        List<Piece> pieces = Board.SelectMany(row => row)
                                  .Where(piece => piece != null)
                                  .ToList();

        BIERMouse.MouseUpdate(pieces);
    }



    public override void GameRender()
    {
        _renderObjects.Clear();
        foreach (var p in Board.SelectMany(row => row))
        {
            if (p != null && p.FilePath != null)
            {
                int renderX, renderY;
                if (p == BIERMouse.DraggedPiece)
                {
                    Vector2 mousePos = Raylib.GetMousePosition();
                    renderX = (int)mousePos.X;
                    renderY = (int)mousePos.Y;
                }
                else
                {
                    renderX = p.X * TILE_SIZE + BOARDXPos;
                    renderY = p.Y * TILE_SIZE + BOARDYPos;
                }

                _renderObjects.Add(new BIERRenderTexture(renderX, renderY, TILE_SIZE, TILE_SIZE, color: WHITE)
                {
                    Texture = _pieceTextureDict[$"{p.FilePath}"]
                });
            }
        }
        BIERRenderer.Render(_renderObjects, BEIGE, CustomPreRenderFuncs, CustomPostRenderFuncs);
    }


    public override void GameDispose()
    {
        _renderObjects.ForEach(o => o.Dispose());
        _pieceTextureDict.Values.ToList().ForEach(t => UnloadTexture(t));
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
