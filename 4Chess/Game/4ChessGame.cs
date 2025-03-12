using _4Chess.Pieces;
using BIERKELLER.BIERGaming;
using BIERKELLER.BIERRender;
using Raylib_CsLo;
using static Raylib_CsLo.Raylib;

namespace _4Chess.Game;

public class _4ChessGame : BIERGame
{
    public const int WINDOW_WIDTH = 1920;
    public const int WINDOW_HEIGHT = 1200;
    public const int BOARD_DIMENSIONS = 8;

    public List<BIERRenderObject> _renderObjects = [];
    public List<List<Piece?>> Board { get; set; } = [];

    public _4ChessGame()
    {
        CustomPreRenderFuncs.Add(RenderBoard);
    }

    public override void GameInit()
    {
        BIERRenderer.Init(WINDOW_WIDTH, WINDOW_HEIGHT, "4Chess");

        // BIERRender-Objekte erst nach BIERRenderer.Init initialisieren, da sie den GL-Context brauchen!

        _renderObjects =
        [
            new BIERRenderRect(200, 200, 300, 100, GREEN),
            new BIERRenderRect(100, 100, 50, 250, RED),
            new BIERRenderTexture("test.png", 300, 200, 400, 300, WHITE)
        ];
    }

    public override void GameUpdate()
    {
        _renderObjects.ForEach(o => o.X -= 1f);
    }

    public override void GameRender()
    {
        BIERRenderer.Render(_renderObjects, GOLD, CustomPreRenderFuncs, CustomPostRenderFuncs);
    }

    public override void GameDispose()
    {
        _renderObjects.ForEach(o => o.Dispose());
    }

    private void RenderBoard()
    {
        var bXPos = WINDOW_WIDTH / 4;
        var bYPos = WINDOW_HEIGHT / 8;

        var tileSize = (WINDOW_WIDTH - (bXPos * 2)) / BOARD_DIMENSIONS;

        char c = 'w';

        for (int x = 0; x < BOARD_DIMENSIONS; x++)
        {
            for (int y = 0; y < BOARD_DIMENSIONS; y++)
            {
                var color = c switch
                {
                    'w' => WHITE,
                    _ => BLACK
                };

                Raylib.DrawRectangle(bXPos + x * tileSize, bYPos + y * tileSize, tileSize, tileSize, color);

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
