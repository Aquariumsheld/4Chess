using Raylib_CsLo;

namespace BIERKELLER.BIERRender;

public class BIERRenderTexture : BIERRenderObject
{
    public Raylib_CsLo.Texture Texture { get; set; }

    public BIERRenderTexture(string path, int x, int y, int w = 0, int h = 0, Color? color = null) : base(x, y, w, h, color)
    {
        Texture = Raylib.LoadTexture($"res/{path}");
    }

    public override void Render()
    {
        var scale = new List<float>()
        {
            W / Texture.width,
            H / Texture.height
        }.Average();
        Raylib.DrawTextureEx(Texture, new System.Numerics.Vector2(X, Y), 0f, scale, Color);
    }

    public override void Dispose()
    {
        Raylib.UnloadTexture(Texture);
    }
}
