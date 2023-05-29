using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse.Steam;
using Verse;

using Steamworks;

namespace YADA;

//[HarmonyPatch(typeof(WorkshopItem), nameof(WorkshopItem.MakeFrom))]
//static class Patch__WorkshopItem__MakeFrom {
//    static void Postfix(ref WorkshopItem __result){
//        if( __result == null || __result is WorkshopItem_Downloading ) return;
//
//        var state = SteamUGC.GetItemState(__result.PublishedFileId);
//        if( (state & (int)EItemState.k_EItemStateNeedsUpdate) != 0 ){
//            Log.Warning("[d] " + __result + " needs update");
//        } else {
//            Log.Warning("[d] " + __result + " " + state);
//        }
//    }
//}
//
//[HarmonyPatch(typeof(WorkshopItems), "RebuildItemsList")]
//static class Patch__WorkshopItems__RebuildItemsList {
//    static void Prefix(){
//        Log.Warning("[d] RebuildItemsList");
//    }
//}
