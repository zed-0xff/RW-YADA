using HarmonyLib;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace YADA;

[HarmonyPatch(typeof(BuildableDef), nameof(BuildableDef.SpecialDisplayStats))]
static class Patch__BuildableDef__SpecialDisplayStats
{
    static IEnumerable<StatDrawEntry> Postfix(IEnumerable<StatDrawEntry> stats, BuildableDef __instance){
        foreach( var x in stats ){
            yield return x;
        }
        if( Prefs.DevMode ){
            if( __instance.graphic?.path != null )
                yield return new StatDrawEntry( VDefOf.YADA_Debug_Graphic, "path", __instance.graphic.path, __instance.graphic.path, 0);

            if( __instance.graphic?.maskPath != null )
                yield return new StatDrawEntry( VDefOf.YADA_Debug_Graphic, "maskPath", __instance.graphic.maskPath, __instance.graphic.maskPath, 0);

            if( __instance.graphic?.data?.graphicClass != null )
                yield return new StatDrawEntry( VDefOf.YADA_Debug_Graphic, "graphicClass",
                        __instance.graphic.data.graphicClass.ToString(),
                        __instance.graphic.data.graphicClass.ToString(), 0);

            if( __instance.graphic?.data?.shaderType != null )
                yield return new StatDrawEntry( VDefOf.YADA_Debug_Graphic, "shaderType",
                        __instance.graphic.data.shaderType.defName,
                        __instance.graphic.data.shaderType.defName, 0);

            if( __instance.designationCategory != null )
                yield return new StatDrawEntry( VDefOf.YADA_Debug, "designationCategory",
                        __instance.designationCategory.defName,
                         __instance.designationCategory.ToString(), 0);

                yield return new StatDrawEntry( VDefOf.YADA_Debug, "altitudeLayer",
                        __instance.altitudeLayer.ToString(),
                        __instance.altitudeLayer.ToString(), 0);
        }
    }
}
