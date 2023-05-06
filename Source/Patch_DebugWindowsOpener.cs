using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using UnityEngine;
using Verse;

namespace zed_0xff.YADA;

[HarmonyPatch(typeof(DebugWindowsOpener), nameof(DebugWindowsOpener.DevToolStarterOnGUI))]
static class Patch_DevToolStarterOnGUI {
    static void Postfix(){
        if( VDefOf.YADA_ToggleDebugLog.KeyDownEvent ) {
            ModConfig.Settings.drawLogOverlay = !ModConfig.Settings.drawLogOverlay;
        }
    }
}