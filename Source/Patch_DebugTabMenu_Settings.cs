using HarmonyLib;
using Verse;
using System.Collections.Generic;
using System.Reflection;

namespace YADA;

static class Patch_DebugTabMenu_Settings {
    // add custom settings to debug menu
    [HarmonyPatch(typeof(DebugTabMenu_Settings), "InitActions")]
    static class Patch_InitActions {
        static void Postfix(DebugTabMenu_Settings __instance) {
            foreach (var fi in DynamicPatch.dynamicSettings) {
                FieldCategory c = fi.GetCustomAttribute<FieldCategory>();
                string category = "YADA";
                if( c?.value != null ){
                    category = c.value;
                }
                __instance.AddNode(fi, category);
            }
        }
    }

    // custom names for custom settings
    [HarmonyPatch(typeof(DebugTabMenu_Settings), "LegibleFieldName")]
    static class Patch_LegibleFieldName {
        static void Postfix(ref string __result, FieldInfo fi){
            FieldLabel l = fi.GetCustomAttribute<FieldLabel>();
            if( l?.value != null ){
                __result = l.value;
            }
        }
    }
}

