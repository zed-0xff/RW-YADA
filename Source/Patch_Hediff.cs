using HarmonyLib;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace zed_0xff.YADA;

[HarmonyPatch(typeof(HediffDef), nameof(HediffDef.SpecialDisplayStats))]
static class Patch__HediffDef__SpecialDisplayStats
{
    static IEnumerable<StatDrawEntry> Postfix(IEnumerable<StatDrawEntry> stats, HediffDef __instance){
        foreach( var x in stats ){
            yield return x;
        }
        if( Prefs.DevMode ){
            foreach( var x in __instance.comps ){
                yield return new StatDrawEntry(
                        VDefOf.YADA_Debug_CompProperties,
                        x.ToString(),
                        x.compClass.ToString(),
                        x + "\ncompClass: " + x.compClass,
                        0);
            }
        }
    }
}
