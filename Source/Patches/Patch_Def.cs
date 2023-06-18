using HarmonyLib;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace YADA;

[HarmonyPatch(typeof(Def), nameof(Def.SpecialDisplayStats))]
static class Patch__Def__SpecialDisplayStats
{
    static IEnumerable<StatDrawEntry> Postfix(IEnumerable<StatDrawEntry> stats, Def __instance){
        if( stats == null ) yield break;

        foreach( var x in stats ){
            yield return x;
        }
        if( Prefs.DevMode && __instance != null ){
            yield return new StatDrawEntry( VDefOf.YADA_Debug, "defName", __instance.defName, __instance.defName, 0);

            if( __instance.modContentPack != null && __instance.modContentPack.SteamAppId != 0 )
                yield return new StatDrawEntry( VDefOf.YADA_Debug, "modID",
                        __instance.modContentPack.SteamAppId.ToString(),
                        __instance.modContentPack.SteamAppId.ToString(), 0);


            if( __instance.modExtensions != null ){
                foreach( DefModExtension ext in __instance.modExtensions ){
                    if( ext == null ) continue;

                    yield return new StatDrawEntry( VDefOf.YADA_Debug_ModExtensions, ext.GetType().ToString(), "", ext.GetType().ToString(), 0);
                }
            }
        }
    }
}
