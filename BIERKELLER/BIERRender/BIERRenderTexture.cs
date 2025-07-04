using Raylib_CsLo;

namespace BIERKELLER.BIERRender;

public class BIERRenderTexture : BIERRenderObject
{
    public Raylib_CsLo.Texture Texture { get; set; }

    public float Scale { get; set; }

    public BIERRenderTexture(int x, int y, int w = 0, int h = 0, string? path = null, float scale = 1f, Color? color = null, bool resize = true) : base(x, y, w, h, color)
    {
        if (path != null)
            if (resize)
                Texture = ResizeNewTexture(path, w, h);
            else
                Texture = Raylib.LoadTexture($"res/{path}");

        Scale = scale;
    }

    public override void Render()
    {
        Raylib.DrawTextureEx(Texture, new System.Numerics.Vector2(X, Y), 0f, Scale, Color);
    }

    public override void Dispose()
    {
        Raylib.UnloadTexture(Texture);
    }

    private static unsafe Texture ResizeNewTexture(string path, int w, int h)
    {
        Image img = Raylib.LoadImage(path);

        Raylib.ImageResize(&img, w, h);

        Texture result = Raylib.LoadTextureFromImage(img);

        Raylib.UnloadImage(img);

        return result;
    }

    public static unsafe Texture ResizeTexture(Texture texture, int w, int h)
    {
        Image img = Raylib.LoadImageFromTexture(texture);

        Raylib.ImageResize(&img, w, h);

        Texture result = Raylib.LoadTextureFromImage(img);

        Raylib.UnloadImage(img);

        return result;
    }
}
