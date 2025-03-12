using Raylib_CsLo;
using static Raylib_CsLo.Raylib;

namespace BIERKELLER.BIERRender;

public class BIERRenderRect : BIERRenderObject
{
    public BIERRenderRect(int x, int y, int w = 0, int h = 0, Color? color = null) : base(x, y, w, h, color)
    {

    }

    public override void Render()
    {
        DrawRectangle(X, Y, W, H, Color);
    }
}
