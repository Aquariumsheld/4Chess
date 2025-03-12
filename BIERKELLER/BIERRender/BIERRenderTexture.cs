using Raylib_CsLo;

namespace BIERKELLER.BIERRender;

public class BIERRenderTexture : BIERRenderObject
{
    public Raylib_CsLo.Texture Texture { get; set; }

    public float Scale { get; set; }

    public BIERRenderTexture(string path, int x, int y, int w = 0, int h = 0, float scale = 1f, Color? color = null) : base(x, y, w, h, color)
    {
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
}
