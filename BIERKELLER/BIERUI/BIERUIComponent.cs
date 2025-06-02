using BIERKELLER.BIERRender;
using System.Collections.Generic;
using System.Drawing;

namespace BIERKELLER.BIERUI;

public abstract class BIERUIComponent
{
    public delegate void ClickEventHandler();
    public List<BIERRenderObject> ComponentRenderObjects { get; set; } = [];
    public List<Raylib_CsLo.Rectangle> ComponentHitboxes { get; set; } = [];
    public bool IsClickable { get; set; }
    public bool IsVisible { get; set; }

    public BIERUIComponent(List<BIERRenderObject> componentRenderObjects, List<Raylib_CsLo.Rectangle> compnentHitboxes, bool isClickable, bool isVisible = true)
    {
        ComponentRenderObjects = componentRenderObjects;
        ComponentHitboxes = compnentHitboxes;
        IsClickable = isClickable;
        IsVisible = isVisible;
    }

    public abstract ClickEventHandler ClickEvent { get; set; }

    public void Show()
    {
        IsVisible = true;
    }

    public void Hide()
    {
        IsVisible = false;
    }

    public void ActvateClickablity()
    {
        IsClickable = true;
    }

    public void DeactvateClickablity()
    {
        IsClickable = false;
    }
}
