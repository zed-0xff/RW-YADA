using HarmonyLib;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace zed_0xff.YADA;

[HarmonyPatch(typeof(GeneDef), nameof(GeneDef.SpecialDisplayStats))]
static class Patch__GeneDef__SpecialDisplayStats
{
    static IEnumerable<StatDrawEntry> Postfix(IEnumerable<StatDrawEntry> stats, GeneDef __instance){
        foreach( var x in stats ){
            yield return x;
        }
        if( Prefs.DevMode ){
            yield return new StatDrawEntry( VDefOf.YADA_Debug, "geneClass", __instance.geneClass.ToString(), __instance.geneClass.ToString(), 0);
        }
    }
}
