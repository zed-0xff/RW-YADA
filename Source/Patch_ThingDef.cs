using HarmonyLib;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace YADA;

[HarmonyPatch(typeof(ThingDef), nameof(ThingDef.SpecialDisplayStats))]
static class Patch__ThingDef__SpecialDisplayStats
{
    static IEnumerable<StatDrawEntry> Postfix(IEnumerable<StatDrawEntry> stats, ThingDef __instance){
        foreach( var x in stats ){
            yield return x;
        }
        if( Prefs.DevMode ){
            if( __instance.thingClass != null )
                yield return new StatDrawEntry(
                        VDefOf.YADA_Debug,
                        "thingClass",
                        __instance.thingClass.ToString(),
                        __instance.thingClass.ToString(),
                        0);
        }
    }
}
