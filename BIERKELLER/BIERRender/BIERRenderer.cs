using Raylib_CsLo;
using static Raylib_CsLo.Raylib;

namespace BIERKELLER.BIERRender;

public static class BIERRenderer
{

    public static void Init(int w = 800, int h = 600, string title = "Mein BIERFrame", int fps = 60)
    {
        InitWindow(w, h, title);
        SetTargetFPS(fps);
    }

    public static void Render(List<BIERRenderObject> renderObjects, Color? bgColor = null)
    {
        var bgColorResolved = bgColor ?? BLACK;

        BeginDrawing();

        ClearBackground(bgColorResolved);

        renderObjects.ForEach(o => o.Render());

        EndDrawing();
    }

    public static void Dispose()
    {
        CloseWindow();
    }
}
