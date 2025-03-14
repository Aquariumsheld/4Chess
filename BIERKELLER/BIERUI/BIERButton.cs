using BIERKELLER.BIERRender;
using Raylib_CsLo;
using static System.Net.Mime.MediaTypeNames;
using System.Drawing;

namespace BIERKELLER.BIERUI;

public class BIERButton : BIERUIComponent
{
    public override ClickEventHandler ClickEvent { get; set; }

    public BIERButton(List<BIERRenderObject> componentRenderObjects, List<Raylib_CsLo.Rectangle> compnentHitboxes, bool isClickable = true, bool isVisible = true) : base(componentRenderObjects, compnentHitboxes, isClickable, isVisible)
    {
        ClickEvent += () => { };
    }

    public BIERButton(string txt, float x, float y, float w, float h, Raylib_CsLo.Color? bgColor, Raylib_CsLo.Color? txtColor, Raylib_CsLo.Font? customFont = null, int? spacing = 3, bool isClickable = true)
        : base([new BIERRenderRect(x, y, w, h, bgColor ?? Raylib.SKYBLUE), 
                new BIERRenderText(txt, GetFittingFontSize(txt, customFont ?? Raylib.GetFontDefault(), w, h, spacing ?? 3), x, y + h/2 - GetFittingFontSize(txt, customFont ?? Raylib.GetFontDefault(), w, h, spacing ?? 3)/2, txtColor ?? Raylib.BLACK, customFont, spacing)],
               [new Raylib_CsLo.Rectangle(x, y, w, h)],
               isClickable)      
    {
        ClickEvent += () => { };
    }

    private static int GetFittingFontSize(string txt, Raylib_CsLo.Font font, float w, float h, int spacing)
    {
        int minFontSize = 1;
        int maxFontSize = 1000;
        int bestFontSize = minFontSize;

        while (minFontSize <= maxFontSize)
        {
            int midFontSize = (minFontSize + maxFontSize) / 2;

            var cFontSize = Raylib.MeasureTextEx(font, txt, midFontSize, spacing);

            if (cFontSize.X <= w && cFontSize.Y <= h)
            {
                bestFontSize = midFontSize;
                minFontSize = midFontSize + 1;
            }
            else
            {
                maxFontSize = midFontSize - 1;
            }
        }
        return bestFontSize;
    }
}
