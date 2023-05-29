using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Verse;

namespace YADA;

// "save image" button on any item icon
[HarmonyPatch(typeof(Widgets), nameof(Widgets.DrawTextureFitted))]
[HarmonyPatch(new []{ typeof(Rect), typeof(Texture), typeof(float), typeof(Vector2), typeof(Rect), typeof(float), typeof(Material) }) ]
static class Patch_Widgets {
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
