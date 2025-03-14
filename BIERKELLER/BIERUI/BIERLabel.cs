using BIERKELLER.BIERRender;
using Raylib_CsLo;
using System.Drawing;

namespace BIERKELLER.BIERUI;

public class BIERLabel : BIERUIComponent
{
    public override ClickEventHandler ClickEvent { get; set; }

    public BIERLabel(List<BIERRenderObject> componentRenderObjects, List<Raylib_CsLo.Rectangle> compnentHitboxes, bool isClickable = false, bool isVisible = true) : base(componentRenderObjects, compnentHitboxes, isClickable, isVisible)
    {
        ClickEvent += () => { };
    }

    public BIERLabel(string txt, float x, float y, int fontSize, Raylib_CsLo.Color color, Raylib_CsLo.Font? customFont = null, float? spacing = 3)
        : base([new BIERRenderText(txt, fontSize, x, y, color, customFont, spacing)],
               [new Raylib_CsLo.Rectangle(
                   x - 2f,
                   y - 2f,
                   Raylib.MeasureTextEx(customFont ?? Raylib.GetFontDefault(), txt, fontSize, spacing ?? 3).X + 3f,
                   Raylib.MeasureTextEx(customFont ?? Raylib.GetFontDefault(), txt, fontSize, spacing ?? 3).Y + 2f
                   )
              ],
               false)      
    {
        ClickEvent += () => { };
    }

    public BIERLabel(string txt, int windowW, int windowH, int fontSize, Raylib_CsLo.Color color, Raylib_CsLo.Font? customFont = null, float? spacing = 3):
        this(txt,
            windowW / 2 - Raylib.MeasureTextEx(customFont ?? Raylib.GetFontDefault(), txt, fontSize, spacing ?? 3).X / 2,
            windowH / 2 - Raylib.MeasureTextEx(customFont ?? Raylib.GetFontDefault(), txt, fontSize, spacing ?? 3).Y / 2,
            fontSize, color, customFont, spacing)
    {

    }
}
