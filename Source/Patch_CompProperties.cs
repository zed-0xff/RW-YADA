using HarmonyLib;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace YADA;

[HarmonyPatch(typeof(CompProperties), nameof(CompProperties.SpecialDisplayStats))]
static class Patch__CompProperties__SpecialDisplayStats
{
    static IEnumerable<StatDrawEntry> Postfix(IEnumerable<StatDrawEntry> stats, CompProperties __instance){
        foreach( var x in stats ){
            yield return x;
        }
        if( Prefs.DevMode ){
            yield return new StatDrawEntry(
                    VDefOf.YADA_Debug_CompProperties,
                    __instance.ToString(),
                    __instance.compClass.ToString(),
                    __instance + "\ncompClass: " + __instance.compClass,
                    0);
        }
    }
}
