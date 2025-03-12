namespace BIERKELLER.BIERRender;

public abstract class BIERRenderObject
{
    public float X { get; set; }
    public float Y { get; set; }
    public float W { get; set; }
    public float H { get; set; }
    public Raylib_CsLo.Color Color { get; protected set; }

    public BIERRenderObject(float x, float y, float w = 0, float h = 0, Raylib_CsLo.Color? color = null)
    {
        X = x;
        Y = y;
        W = w;
        H = h;
        Color = color ?? Raylib_CsLo.Raylib.WHITE;
    }

    public abstract void Render();
    public abstract void Dispose();
}
