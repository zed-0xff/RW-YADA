using HarmonyLib;
using Verse;
#if RW15
using LudeonTK;
#endif

namespace YADA;

[HarmonyPatch(typeof(DebugWindowsOpener), nameof(DebugWindowsOpener.DevToolStarterOnGUI))]
static class Patch_DevToolStarterOnGUI {
    static void Postfix(){
        if( VDefOf.YADA_ToggleDebugLog.KeyDownEvent ) {
            ModConfig.Settings.drawLogOverlay = !ModConfig.Settings.drawLogOverlay;
        }
    }
}
