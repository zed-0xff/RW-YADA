using HarmonyLib;
using RimWorld;

namespace YADA;

// draw log on all loading/menu screens, but before drawing any other widgets, so they look better
[HarmonyPatch(typeof(UI_BackgroundMain), nameof(UI_BackgroundMain.BackgroundOnGUI))]
static class Patch__UI_BackgroundMain__BackgroundOnGUI {
    static void Postfix(){
        if( ModConfig.Settings.drawLogOverlayWhileLoading ){
            LogOverlay.Draw();
        }
    }
}
