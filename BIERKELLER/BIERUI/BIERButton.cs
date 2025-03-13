using BIERKELLER.BIERRender;
using Raylib_CsLo;

namespace BIERKELLER.BIERUI;

public class BIERButton : BIERUIComponent
{
    public override ClickEventHandler ClickEvent { get; set; }

    public BIERButton(List<BIERRenderObject> componentRenderObjects, List<Raylib_CsLo.Rectangle> compnentHitboxes, bool isClickable = true, bool isVisible = true) : base(componentRenderObjects, compnentHitboxes, isClickable, isVisible)
    {
        ClickEvent += () => { };
    }
}
