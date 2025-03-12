
using BIERKELLER.BIERRender;

namespace BIERKELLER.BIERGaming;

public abstract class BIERGame
{

    public void Run()
    {
        GameInit();
        while (!Raylib_CsLo.Raylib.WindowShouldClose())
        {
            GameUpdate();
            GameRender();
        }
        GameDispose();
        BIERRenderer.Dispose();
    }

    public abstract void GameInit();
    public abstract void GameUpdate();
    public abstract void GameRender();
    public abstract void GameDispose();
}

