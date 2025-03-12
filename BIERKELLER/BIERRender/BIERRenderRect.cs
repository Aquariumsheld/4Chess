using Raylib_CsLo;
using static Raylib_CsLo.Raylib;

namespace BIERKELLER.BIERRender;

public class BIERRenderRect : BIERRenderObject
{
    public BIERRenderRect(float x, float y, float w = 0, float h = 0, Color? color = null) : base(x, y, w, h, color)
    {

    }

    public override void Render()
    {
        DrawRectangle((int)X, (int)Y, (int)W, (int)H, Color);
    }

    public override void Dispose()
    {

    }
}
