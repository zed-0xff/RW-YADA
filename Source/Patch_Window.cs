using HarmonyLib;
using UnityEngine;
using Verse;

namespace zed_0xff.YADA;

[HarmonyPatch(typeof(Window), "InnerWindowOnGUI")]
static class Patch__Window__InnerWindowOnGUI {
    static void Postfix(Window __instance){
        if( !ModConfig.Settings.showWindowClass ) return;
        if( !Prefs.DevMode ) return;

        // not looks good
        if( __instance is FloatMenu || __instance is ImmediateWindow ) return;

        var prevFont = Text.Font;
        var prevColor = GUI.color;

        Text.Font = GameFont.Tiny;
        Rect rect = __instance.windowRect.AtZero();
        rect.x += 3;
        GUI.color = ColorLibrary.Grey;
        Widgets.Label(rect, __instance.GetType().ToString());

        Text.Font = prevFont;
        GUI.color = prevColor;
    }
}
