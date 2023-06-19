using HarmonyLib;
using RimWorld;
using Verse;

namespace YADA;

[HarmonyPatch(typeof(DebugTools), nameof(DebugTools.DebugToolsOnGUI))]
static class HideOnScreenShot {
    static bool Prefix(){
        return !ScreenshotMaker.Queued;
    }
}

