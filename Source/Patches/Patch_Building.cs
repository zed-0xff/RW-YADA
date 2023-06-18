using HarmonyLib;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace YADA;

[HarmonyPatch(typeof(Building), nameof(Building.GetGizmos))]
static class Patch_Building {
    static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Building __instance) {
        foreach( Gizmo gizmo in __result ){
            yield return gizmo;
        }
        if( Prefs.DevMode && __instance.Stuff != null ){
            Command command = BuildCopyCommandUtility.BuildCopyCommand(
                    __instance.def,
                    null,
                    __instance.StyleSourcePrecept as Precept_Building,
                    __instance.StyleDef, styleOverridden: true
                    );
            if (command != null)
            {
                yield return command;
            }
        }
    }
}

