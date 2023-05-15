using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using UnityEngine;
using Verse;

namespace zed_0xff.YADA;

[HarmonyPatch(typeof(WindowStack), nameof(WindowStack.Add))]
static class Patch__WindowStack__Add {
    static void Prefix(Window window){
        if( !ModConfig.Settings.removeModUploadDelay ) return;

        if( window is Dialog_MessageBox mb && mb.text == "ConfirmContentAuthor".Translate() && mb.interactionDelay == 6f ){
            mb.interactionDelay = 0;
        }
    }
}
