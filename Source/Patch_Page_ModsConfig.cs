using HarmonyLib;
using RimWorld;
using UnityEngine;
using System.Collections.Generic;
using System;
using Verse;

using Steamworks;

namespace zed_0xff.YADA;

//[HarmonyPatch(typeof(Page_ModsConfig), "DoModInfo")]
//static class Patch_Page_ModsConfig {
//
//    public static string flags2string(Type t, uint value)
//    {
//        List<string> list = new List<string>();
//        foreach(var e in Enum.GetValues(t)) {
//            if ((((int)e) & value) != 0) list.Add(e.ToString());
//        }
//        return String.Join(" | ", list);
//    }
//
//    static void Postfix(Rect r, ModMetaData mod){
//        var state = SteamUGC.GetItemState(mod.GetPublishedFileId());
//        if( Widgets.ButtonText(new Rect(r.x, r.y, r.width, 24), flags2string(typeof(EItemState), state) )){
//            bool b = SteamUGC.DownloadItem( mod.GetPublishedFileId(), true );
//            if( b ){
//                Callback<DownloadItemResult_t>.Create(delegate(DownloadItemResult_t result){
//                        Log.Warning("[d] " + result.m_unAppID + " " + result.m_nPublishedFileId + " " + result.m_eResult);
//                        });
//            }
//        }
//    }
//}
