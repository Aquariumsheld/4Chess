using _4Chess.Pieces;
using BIERKELLER.BIERGaming;
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


        Board =
        [
            [new Pawn(0, 0, Piece.Color.Black, this), new Pawn(1, 0, Piece.Color.Black, this), new Pawn(0, 1, Piece.Color.White, this), new Rook(0, 2, Piece.Color.White, this), new Pawn(0, 0, Piece.Color.Black, this), new Queen(7, 6, Piece.Color.Black, this), new Pawn(0, 0, Piece.Color.Black, this), new Pawn(0, 0, Piece.Color.Black, this)]
        ];
    }

    public override void GameUpdate()
    {
        _renderObjects.ForEach(o => o.X -= 1f);
    }

    public override void GameRender()
    {
        _renderObjects.Clear();
        Board.SelectMany(p => p).ToList().ForEach(p =>
        {
            if (p != null && p.FilePath != null)
                _renderObjects.Add(new BIERRenderTexture(p.FilePath, p.X * TILE_SIZE + BOARDXPos, p.Y * TILE_SIZE + BOARDYPos, TILE_SIZE, TILE_SIZE, 1f, WHITE));
        });
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
