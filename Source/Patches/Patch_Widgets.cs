using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using Verse;

namespace YADA;

// "save image" button on any item icon; applied manually so we skip when overload is missing (e.g. 1.6)
static class Patch_Widgets {
    static readonly Type[] DrawTextureFittedArgTypes = { typeof(Rect), typeof(Texture), typeof(float), typeof(Vector2), typeof(Rect), typeof(float), typeof(Material) };

    internal static void ApplyIfTargetExists(Harmony harmony) {
        var target = AccessTools.Method(typeof(Widgets), nameof(Widgets.DrawTextureFitted), DrawTextureFittedArgTypes);
        if (target == null) return;
        var postfix = AccessTools.Method(typeof(Patch_Widgets), nameof(Postfix));
        harmony.Patch(target, postfix: new HarmonyMethod(postfix));
    }

    static void Postfix(Rect outerRect, Texture tex){
        if( !Prefs.DevMode ) return;
        if( !ModConfig.Settings.textureSaver ) return;

        if( Event.current.type == EventType.MouseDown && Event.current.button == 1 && Mouse.IsOver(outerRect) && tex is Texture2D t2d ){
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            string fname = tex.name + ".png";
            list.Add(new FloatMenuOption("Save as " + fname, delegate
                        {
                        fname = ModConfig.Settings.textureSavePath + Path.DirectorySeparatorChar.ToString() + fname;
                        TextureAtlasHelper.WriteDebugPNG(t2d, fname);
                        Messages.Message(fname + " saved", MessageTypeDefOf.PositiveEvent, historical: false);
                        }));
            Find.WindowStack.Add(new FloatMenu(list));
        }
    }
}
