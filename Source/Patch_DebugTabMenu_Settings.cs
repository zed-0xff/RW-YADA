using HarmonyLib;
using Verse;
using System.Collections.Generic;

namespace zed_0xff.YADA;

static class Patch_DebugTabMenu_Settings {
    [HarmonyPatch(typeof(DebugTabMenu_Settings), "InitActions")]
    static class Patch_InitActions {
        static void Postfix(DebugTabMenu_Settings __instance) {
            foreach (var fieldInfo in typeof(Yada_DebugSettings).GetFields())
            {
                __instance.AddNode(fieldInfo, "YADA");
            }
        }
    }
}

