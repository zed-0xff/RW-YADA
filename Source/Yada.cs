using Verse;
using RimWorld.Planet;
using System.Reflection;
using System;
using HarmonyLib;

namespace YADA;

class Yada : WorldComponent {
    public Yada(World w) : base(w) {
    }

    public void Save(Type t, FieldInfo[] fields = null){
        if( fields == null )
            fields = t.GetFields();
        foreach (FieldInfo fi in fields){
            if( fi.FieldType == typeof(bool ) ){
                bool b = (bool)fi.GetValue(null);
                Scribe_Values.Look(ref b, t.Name + "." + fi.Name, forceSave: true);
            }
        }
    }

    public void Load(Type t, FieldInfo[] fields = null){
        if( fields == null )
            fields = t.GetFields();
        foreach (FieldInfo fi in fields){
            if( fi.FieldType == typeof(bool ) ){
                bool b = (bool)fi.GetValue(null);
                Scribe_Values.Look(ref b, t.Name + "." + fi.Name, b);
                if( b != (bool)fi.GetValue(null) ){
                    fi.SetValue(null, b);
                    MethodInfo method = fi.DeclaringType.GetMethod(fi.Name + "Toggled", BindingFlags.Static | BindingFlags.Public);
                    if (method != null){
                        method.Invoke(null, null);
                    }
                }
            }
        }
    }

    public static FieldInfo fiDebugWindowsOpener = AccessTools.Field(typeof(UIRoot), "debugWindowOpener");
    private static FieldInfo fiCanAutoOpen = AccessTools.Field(typeof(EditWindow_Log), "canAutoOpen");

    public override void ExposeData()
    {
        base.ExposeData();
        if( !Prefs.DevMode ) return;

        switch (Scribe.mode){
            case LoadSaveMode.Saving:
                Save(typeof(DebugSettings));
                Save(typeof(DebugViewSettings));
                Save(typeof(DynamicPatch), DynamicPatch.dynamicSettings.ToArray());
                if( ModConfig.Settings.saveDebugLogAutoOpen ){
                    bool b = (bool)fiCanAutoOpen.GetValue(null);
                    Scribe_Values.Look(ref b, "EditWindow_Log.canAutoOpen", forceSave: true);
                }
                break;
            case LoadSaveMode.LoadingVars:
                Load(typeof(DebugSettings));
                Load(typeof(DebugViewSettings));
                Load(typeof(DynamicPatch), DynamicPatch.dynamicSettings.ToArray());
                if( ModConfig.Settings.saveDebugLogAutoOpen ){
                    bool b = true;
                    Scribe_Values.Look(ref b, "EditWindow_Log.canAutoOpen", true);
                    fiCanAutoOpen.SetValue(null, b);
                }
                break;
            case LoadSaveMode.PostLoadInit:
                LongEventHandler.QueueLongEvent(delegate
                        {
                        var opener = (DebugWindowsOpener)fiDebugWindowsOpener.GetValue(Find.UIRoot);
                        if( opener != null ){
                            opener.TryOpenOrClosePalette();
                        }
                        }, null, doAsynchronously: false, null);
                break;
        }

        // alternate way:
        // Scribe_Values.Look(ref DebugViewSettings.drawLightingOverlay, "drawLightingOverlay", true);
    }
}
