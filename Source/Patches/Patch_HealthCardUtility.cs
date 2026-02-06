using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;
using UnityEngine;

namespace YADA;

[HarmonyPatch(typeof(HealthCardUtility), "DrawHediffRow")]
static class Patch_DrawHediffRow {
    static readonly MethodInfo m_getShowDevGizmos = AccessTools.PropertyGetter(typeof(DebugSettings), nameof(DebugSettings.ShowDevGizmos));
    static readonly MethodInfo m_1 = AccessTools.Method(typeof(Patch_DrawHediffRow), nameof(Patch_DrawHediffRow.m1));
    // Nested type name is compiler-generated and varies by game version (e.g. c__DisplayClass31_1); resolve by presence of iconsRect
    static readonly FieldInfo f_iconsRect = FindIconsRectField();

    static FieldInfo FindIconsRectField() {
        foreach (var nt in typeof(HealthCardUtility).GetNestedTypes(AccessTools.all)) {
            if (!nt.Name.Contains("c__DisplayClass")) continue;
            var f = AccessTools.Field(nt, "iconsRect");
            if (f != null) return f;
        }
        return null;
    }

    static void m1(List<GenUI.AnonymousStackElement> list, Hediff localHediff, Rect iconsRect){
        if ( localHediff == null || iconsRect == null ) return;
        if ( localHediff.def.minSeverity == localHediff.def.maxSeverity && localHediff.Severity == localHediff.def.maxSeverity ) return;

        const float W = 20f;
        const float H = 20f;

        float delta = 0.1f * GenUI.CurrentAdjustmentMultiplier();

        list.Add(new GenUI.AnonymousStackElement {
            drawer = delegate(Rect r)
            {
                float num12 = iconsRect.width - (r.x - iconsRect.x) - W;
                r = new Rect(iconsRect.x + num12, r.y, W, H);
                GUI.color = Color.yellow;
                TooltipHandler.TipRegion(r, () => "DEV: increment severity by "+delta+"\nvalue="+localHediff.Severity.ToString("F2"), 1071045646);
                if (GUI.Button(r, TexButton.Plus))
                {
                    localHediff.Severity += delta;
                }
                GUI.color = Color.white;
            },
            width = W
        });
        list.Add(new GenUI.AnonymousStackElement {
            drawer = delegate(Rect r)
            {
                float num12 = iconsRect.width - (r.x - iconsRect.x) - W;
                r = new Rect(iconsRect.x + num12, r.y, W, H);
                GUI.color = Color.green;
                TooltipHandler.TipRegion(r, () => "DEV: decrement severity by "+delta+"\nvalue="+localHediff.Severity.ToString("F2"), 1071045647);
                if (GUI.Button(r, TexButton.Minus))
                {
                    localHediff.Severity -= delta;
                }
                GUI.color = Color.white;
            },
            width = W
        });
    }

    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
        int state = 0;
        foreach (var code in instructions){
            yield return code;
            switch(state){
                case 0:
                    if( code.opcode == OpCodes.Call && (code.operand as MethodInfo) == m_getShowDevGizmos )
                        state++;
                    break;
                case 1:
                    if( code.opcode == OpCodes.Callvirt && f_iconsRect != null ){
                        // callvirt List<AnonymousStackElement>.Add
                        if( code.operand?.ToString() == "Void Add(AnonymousStackElement)") {
                            yield return new CodeInstruction(OpCodes.Ldloc_S, 18); // list
                            yield return new CodeInstruction(OpCodes.Ldloc_S, 14); // localHediff
                            yield return new CodeInstruction(OpCodes.Ldloc_S, 12); // display class instance
                            yield return new CodeInstruction(OpCodes.Ldfld, f_iconsRect);
                            yield return new CodeInstruction(OpCodes.Call, m_1);
                            state = -1; // patched
                        } else {
                            state = -1; // code changed, do not patch
                        }
                    }
                    break;
            }
        }
    }
}
