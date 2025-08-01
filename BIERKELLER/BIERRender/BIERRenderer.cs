﻿using Raylib_CsLo;
using static Raylib_CsLo.Raylib;

namespace BIERKELLER.BIERRender;

public static class BIERRenderer
{

    public static void Init(int w = 800, int h = 600, string title = "Mein BIERFrame", int fps = 60, bool vsync = false, string? iconPath = null)
    {
        InitWindow(w, h, title);
        SetTargetFPS(vsync ? GetMonitorRefreshRate(GetCurrentMonitor()) : fps);
        if (iconPath != null)
        {
            Image icon = LoadImage(iconPath);
            SetWindowIcon(icon);
            UnloadImage(icon);
        }
        MaximizeWindow();
    }

    public static void Render(List<BIERRenderObject> renderObjects, Color? bgColor = null, List<Action>? cPreFuncs = null, List<Action>? cPostFuncs = null)
    {
        var bgColorResolved = bgColor ?? BLACK;


        BeginDrawing();

        ClearBackground(bgColorResolved);

        cPreFuncs?.ForEach(a => a.Invoke());

        renderObjects.ForEach(o => o.Render());

        cPostFuncs?.ForEach(a => a.Invoke());

        EndDrawing();
    }

    public static void Dispose()
    {
        CloseWindow();
    }
}
