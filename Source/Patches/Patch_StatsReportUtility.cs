using HarmonyLib;
using RimWorld;
using Verse;
using Verse.Sound;
using UnityEngine;

namespace YADA;

[HarmonyPatch(typeof(StatsReportUtility), "DrawStatsWorker")]
static class Patch_StatsReportUtility {
    static void Postfix(Rect rect, StatDrawEntry ___selectedEntry, Thing optionalThing) {
        if( !Prefs.DevMode ) return;
        if( ___selectedEntry == null ) return;
        if( rect == null ) return;

        Rect buttonRect = new Rect(rect.x+rect.width-180, rect.y-20, 180, 20);
        if (Widgets.ButtonText(buttonRect, "CopyBillTip".Translate())) {
            SoundDefOf.Tick_High.PlayOneShotOnCamera();
            StatRequest statRequest = (___selectedEntry.hasOptionalReq ? ___selectedEntry.optionalReq : ((optionalThing == null) ? StatRequest.ForEmpty() : StatRequest.For(optionalThing)));
            GUIUtility.systemCopyBuffer = ___selectedEntry.GetExplanationText(statRequest);
        }
    }
}
