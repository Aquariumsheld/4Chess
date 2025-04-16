using BIERKELLER.BIERRender;
using Raylib_CsLo;
using static System.Net.Mime.MediaTypeNames;

namespace BIERKELLER.BIERUI;

public class BIERInput : BIERUIComponent
{
    public override ClickEventHandler ClickEvent { get; set; } = () => { };

    public string TextValue { get; set; } = "";
    public Raylib_CsLo.Font? CustomFont;
    private float X;
    private float Y;
    private float W;
    private float H;
    private int Spacing;

    public BIERInput(List<BIERRenderObject> componentRenderObjects, List<Rectangle> compnentHitboxes, bool isClickable = false, bool isVisible = true) : base(componentRenderObjects, compnentHitboxes, isClickable, isVisible)
    {
        
    }

    public BIERInput(string txt, float x, float y, float w, float h, Raylib_CsLo.Color? bgColor, Raylib_CsLo.Color? txtColor, Raylib_CsLo.Font? customFont = null, int? spacing = 3)
        : base([new BIERRenderRect(x, y, w, h, bgColor ?? Raylib.SKYBLUE),
                new BIERRenderText(txt, GetFittingFontSize(txt, customFont ?? Raylib.GetFontDefault(), w, h, spacing ?? 3), x, y + h/2 - GetFittingFontSize(txt, customFont ?? Raylib.GetFontDefault(), w, h, spacing ?? 3)/2, txtColor ?? Raylib.BLACK, customFont, spacing)],
               [new Raylib_CsLo.Rectangle(x, y, w, h)], false)
    {
        TextValue = txt;
        CustomFont = customFont;
        W = w; 
        H = h;
        Spacing = spacing ?? 3;
    }

    public void SyncText()
    {
        ComponentRenderObjects.Where(x => x is BIERRenderText).ToList().ForEach(x =>
        {
            var y = (BIERRenderText)x;
            y.Text = TextValue;
            y.FontSize = GetFittingFontSize(TextValue, CustomFont ?? Raylib.GetFontDefault(), W, H, Spacing);
        });
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
