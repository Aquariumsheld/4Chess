namespace BIERKELLER.BIERRender;

public abstract class BIERRenderObject
{
    public int X { get; set; }
    public int Y { get; set; }
    public int W { get; set; }
    public int H { get; set; }
    public Raylib_CsLo.Color Color { get; protected set; }

    public BIERRenderObject(int x, int y, int w = 0, int h = 0, Raylib_CsLo.Color? color = null)
    {
        X = x;
        Y = y;
        W = w;
        H = h;
        Color = color ?? Raylib_CsLo.Raylib.WHITE;
    }

    public abstract void Render();
}
