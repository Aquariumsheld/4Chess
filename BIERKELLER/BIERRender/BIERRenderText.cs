using Raylib_CsLo;
using System.Numerics;
using System.Text;

namespace BIERKELLER.BIERRender;

public class BIERRenderText : BIERRenderObject
{
    public string Text { get; set; }
    public int FontSize { get; set; }
    public Font? CustomFont { get; set; }
    public float? Spacing { get; set; }

    public BIERRenderText(string text, int fontSize, float x, float y, Color? color = null, Font? customFont = null, float? spacing = 3) : base(x, y, 1f, 1f, color)
    {
        Text = text;
        FontSize = fontSize;
        CustomFont = customFont;
        Spacing = spacing;
    }

    public override void Dispose()
    {
        var resolvedFont = CustomFont ?? new Font();
        Raylib.UnloadFont(resolvedFont);
    }

    public unsafe override void Render()
    {
        var encodedText = Encoding.ASCII.GetBytes(Text);

        fixed (byte* encodedTextPtrUnsigned = encodedText)
        {
            sbyte* encodedTextPtr = (sbyte*)encodedTextPtrUnsigned;

            if (CustomFont == null)
                Raylib.DrawText(Text, X, Y, FontSize, Color);
            else
            {
                var resolvedFont = CustomFont ?? new Font();
                Raylib.DrawTextEx(resolvedFont, encodedTextPtr, new Vector2(X, Y), FontSize, Spacing ?? 3, Color);
            }    
        }
    }
}
