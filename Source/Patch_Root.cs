using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using UnityEngine;
using Verse;

namespace zed_0xff.YADA;

[HarmonyPatch(typeof(Root), nameof(Root.OnGUI))]
static class Patch__Root__OnGUI {
    static void Postfix(){
        if( !ModConfig.Settings.drawLogOverlay ) return;
        if( Event.current.type != EventType.Repaint ) return;

        Rect viewRect = new Rect(
                ModConfig.Settings.logX,
                ModConfig.Settings.logY,
                ModConfig.Settings.logW,
                ModConfig.Settings.logH
                );
        float width = ModConfig.Settings.logW;
        GUI.color = new Color(ModConfig.Settings.logHue, ModConfig.Settings.logHue, ModConfig.Settings.logHue, ModConfig.Settings.logOpa);
        GenUI.SetLabelAlign(TextAnchor.UpperLeft);

        Text.Font = GameFont.Tiny;
        float x0 = ModConfig.Settings.logX;
        float y0 = ModConfig.Settings.logY;
        float y = ModConfig.Settings.logY + ModConfig.Settings.logH;
        foreach (LogMessage message in Log.Messages.Reverse()){
            string text = message.repeats == 1 ? message.text : ("[" + message.repeats + "] " + message.text);
            if (text.Length > 1000) {
                text = text.Substring(0, 1000);
            }
            float lineHeight = Math.Min(Text.TinyFontSupported ? 30f : Text.LineHeight, Text.CalcHeight(text, width));
            y -= lineHeight*ModConfig.Settings.logLineSpacing;
            Rect rect = new Rect(x0, y, width, lineHeight);
            Widgets.Label(rect, text);
            //GUI.Label(rect, text);
            if( y <= y0 ) break;
        }
        Text.Font = GameFont.Small;
        GUI.color = Color.white;
        GenUI.ResetLabelAlign();
    }
}
