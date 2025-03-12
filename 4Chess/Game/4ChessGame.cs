using BIERKELLER.BIERGaming;
using BIERKELLER.BIERRender;
using static Raylib_CsLo.Raylib;

namespace _4Chess.Game;

public class _4ChessGame : BIERGame
{
    public const int WINDOW_WIDTH = 800;
    public const int WINDOW_HEIGHT = 600;

    public List<BIERRenderObject> _renderObjects = [];

    public _4ChessGame()
    {
        
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
        _renderObjects.ForEach(o => o.X += 1);
    }

    public override void GameRender()
    {
        BIERRenderer.Render(_renderObjects, GOLD);
    }

    public override void GameDispose()
    {
        _renderObjects.ForEach(o => o.Dispose());
    }
}
