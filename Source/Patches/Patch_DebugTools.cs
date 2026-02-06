using HarmonyLib;
using RimWorld;
using Verse;
#if RW15
using LudeonTK;
#endif

namespace YADA;

[HarmonyPatch(typeof(DebugTools), nameof(DebugTools.DebugToolsOnGUI))]
static class HideOnScreenShot {
    static bool Prefix(){
        return !ScreenshotMaker.Queued;
    }
}

