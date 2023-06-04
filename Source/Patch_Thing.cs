using HarmonyLib;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace YADA;

[HarmonyPatch(typeof(Thing), nameof(Thing.SpecialDisplayStats))]
static class Patch__Thing__SpecialDisplayStats
{
    static IEnumerable<StatDrawEntry> Postfix(IEnumerable<StatDrawEntry> stats, Thing __instance){
        foreach( var x in stats ){
            yield return x;
        }
        if( Prefs.DevMode ){
            if( __instance.Graphic != null )
                yield return new StatDrawEntry( VDefOf.YADA_Debug_Graphic, "class",
                        __instance.Graphic.GetType().Name,
                        __instance.Graphic.ToString(), 0);
        }
    }
}

//[HarmonyPatch(typeof(Thing), nameof(Thing.SpawnSetup))]
//static class Patch__Thing__VerboseLoad
//{
//    static void Prefix(Thing __instance, bool respawningAfterLoad){
//        // TODO: config
//        if( Prefs.DevMode && respawningAfterLoad ){
//            Log.Message("[d] SpawnSetup: " + __instance);
//        }
//    }
//}
