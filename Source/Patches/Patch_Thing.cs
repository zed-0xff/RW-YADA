using HarmonyLib;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace YADA;

[HarmonyPatch(typeof(Thing), nameof(Thing.SpecialDisplayStats))]
static class Patch__Thing__SpecialDisplayStats
{
    static IEnumerable<StatDrawEntry> Postfix(IEnumerable<StatDrawEntry> stats, Thing __instance){
        if( stats == null ) yield break;

        foreach( var x in stats ){
            yield return x;
        }
        if( Prefs.DevMode && __instance != null ){
            yield return new StatDrawEntry( VDefOf.YADA_Debug, "ThingID",
                    __instance.ThingID,
                    __instance.ThingID, 0);

            if( __instance.Graphic != null )
                yield return new StatDrawEntry( VDefOf.YADA_Debug_Graphic, "class",
                        __instance.Graphic.GetType().Name,
                        __instance.Graphic.ToString(), 0);
        }
    }
}
