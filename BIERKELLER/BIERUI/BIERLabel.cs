using BIERKELLER.BIERRender;
using System.Drawing;

namespace BIERKELLER.BIERUI;

public class BIERLabel : BIERUIComponent
{
    public override ClickEventHandler ClickEvent { get; set; }

    public BIERLabel(List<BIERRenderObject> componentRenderObjects, List<Raylib_CsLo.Rectangle> compnentHitboxes, bool isClickable = false, bool isVisible = true) : base(componentRenderObjects, compnentHitboxes, isClickable, isVisible)
    {
        ClickEvent += () => Console.WriteLine("Label-Click!");
    }
}
