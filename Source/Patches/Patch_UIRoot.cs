using HarmonyLib;
using RimWorld;
using Verse;

namespace YADA;

[HarmonyPatch(typeof(UIRoot), nameof(UIRoot.UIRootUpdate))]
static class Patch_MakeScreenshot {
    static void Prefix() {
        ScreenshotMaker.Update();
    }
}

