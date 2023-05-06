using Verse;
using RimWorld.Planet;
using System.Reflection;
using System;
using HarmonyLib;

namespace zed_0xff.YADA;

class Yada : WorldComponent {
    public Yada(World w) : base(w) {
    }

    public void Save(Type t){
        FieldInfo[] fields = t.GetFields();
        foreach (FieldInfo fi in fields){
            if( fi.FieldType == typeof(bool ) ){
                bool b = (bool)fi.GetValue(t);
                Scribe_Values.Look(ref b, t.Name + "." + fi.Name, forceSave: true);
            }
        }
    }

    public void Load(Type t){
        FieldInfo[] fields = t.GetFields();
        foreach (FieldInfo fi in fields){
            if( fi.FieldType == typeof(bool ) ){
                bool b = (bool)fi.GetValue(t);
                Scribe_Values.Look(ref b, t.Name + "." + fi.Name, b);
                if( b != (bool)fi.GetValue(t) ){
                    fi.SetValue(null, b);
                    MethodInfo method = fi.DeclaringType.GetMethod(fi.Name + "Toggled", BindingFlags.Static | BindingFlags.Public);
                    if (method != null){
                        method.Invoke(null, null);
                    }
                }
            }
        }
    }

    public static FieldInfo UIRoot_DebugWindowsOpener = AccessTools.Field(typeof(UIRoot), "debugWindowOpener");

    public override void ExposeData()
    {
        base.ExposeData();
        if( !Prefs.DevMode ) return;

        Log.Warning("[d] scribe " + Scribe.mode);

        switch (Scribe.mode){
            case LoadSaveMode.Saving:
                Save(typeof(DebugSettings));
                Save(typeof(DebugViewSettings));
                Save(typeof(Yada_DebugSettings));
                break;
            case LoadSaveMode.LoadingVars:
                Load(typeof(DebugSettings));
                Load(typeof(DebugViewSettings));
                Load(typeof(Yada_DebugSettings));
                LongEventHandler.QueueLongEvent(delegate
                        {
                        var opener = (DebugWindowsOpener)UIRoot_DebugWindowsOpener.GetValue(Find.UIRoot);
                        if( opener != null ){
                            opener.TryOpenOrClosePalette();
                        }
                        }, null, doAsynchronously: false, null);
                break;
            case LoadSaveMode.PostLoadInit:
                break;
        }

        // alternate way:
        // Scribe_Values.Look(ref DebugViewSettings.drawLightingOverlay, "drawLightingOverlay", true);
    }
}
