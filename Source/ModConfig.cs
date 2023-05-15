using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace zed_0xff.YADA;

public class YADASettings : ModSettings {
    public bool drawLogOverlay = false;

    public int logX = 8;
    public int logY = 105;
    public int logW = 1024;
    public int logH = 732;
    public float logHue = 0.75f;
    public float logOpa = 0.89f;
    public float logLineSpacing = 0.88f;

    public bool saveDebugLogAutoOpen = true;

    public override void ExposeData() {
        Scribe_Values.Look(ref drawLogOverlay, "drawLogOverlay", false);
        Scribe_Values.Look(ref logX, "log.x", 8);
        Scribe_Values.Look(ref logY, "log.y", 105);
        Scribe_Values.Look(ref logW, "log.width", 1024);
        Scribe_Values.Look(ref logH, "log.height", 732);
        Scribe_Values.Look(ref logHue, "log.hue", 0.75f);
        Scribe_Values.Look(ref logOpa, "log.opacity", 0.89f);
        Scribe_Values.Look(ref logLineSpacing, "log.lineSpacing", 0.88f);

        Scribe_Values.Look(ref saveDebugLogAutoOpen, "saveDebugLogAutoOpen", true);
        base.ExposeData();
    }
}

public class ModConfig : Mod {

    public static string RootDir;

    public static YADASettings Settings { get; private set; }

    public ModConfig(ModContentPack content) : base(content) {
        RootDir = content.RootDir;
        Settings = GetSettings<YADASettings>();
        // apply YADA core patches here, before Init.Init() to hook Root.OnGUI early
        Harmony harmony = new Harmony("zed_0xff.YADA");
        harmony.PatchAll();
    }

    public override void DoSettingsWindowContents(Rect inRect) {
        var tabRect = new Rect(inRect) {
            y = inRect.y + 40f
        };
        var mainRect = new Rect(inRect) {
            height = inRect.height - 40f,
            y = inRect.y + 40f
        };

        Widgets.DrawMenuSection(mainRect);

        var tabs = new List<TabRecord> {
            new TabRecord("General".Translate(), () => { PageIndex = 0; WriteSettings(); }, PageIndex == 0),
            new TabRecord("Log overlay", ()         => { PageIndex = 1; WriteSettings(); }, PageIndex == 1),
            new TabRecord("Tools", ()               => { PageIndex = 2; WriteSettings(); }, PageIndex == 2),
        };

        TabDrawer.DrawTabs(tabRect, tabs);

        switch (PageIndex)
        {
            case 0:
                draw_general(mainRect.ContractedBy(15f));
                break;
            case 1:
                draw_log(mainRect.ContractedBy(15f));
                break;
            case 2:
                draw_tools(mainRect.ContractedBy(15f));
                break;
            default:
                break;
        }
        base.DoSettingsWindowContents(inRect);
    }

    string fqmn = "StorytellerUtility.DefaultThreatPointsNow";
    string msg;

    void draw_tools(Rect inRect){
        Listing_Standard l = new Listing_Standard();
        l.Begin(inRect);

        string error = null;
        fqmn = l.TextEntryLabeled("ClassName.MethodName: ", fqmn);
        if( l.ButtonTextLabeled("", "disasm to debug log") ){
            MethodInfo mi = Utils.fqmnToMethodInfo(fqmn, out error);
            if( mi != null && error == null ){
                StringBuilder sb = new StringBuilder();
                msg = "wrote " + Utils.Disasm(mi, sb) + " instructions to debug log";
                Log.Message(sb.ToString());
            }
        }
        if( error != null ){
            msg = error.Colorize(Color.red);
        }
        l.LabelDouble("", msg);

        l.End();
    }

    void draw_general(Rect inRect){
        Listing_Standard l = new Listing_Standard();
        l.Begin(inRect);

        l.CheckboxLabeled("Save DebugLog auto-open value", ref Settings.saveDebugLogAutoOpen);

        l.End();
    }

    // from Dialog_KeyBindings.cs
    private void SettingButtonClicked(KeyBindingDef keyDef, KeyPrefs.BindingSlot slot)
    {
        if (Event.current.button == 0)
        {
            Find.WindowStack.Add(new Dialog_DefineBinding(KeyPrefs.KeyPrefsData, keyDef, slot));
            Event.current.Use();
        }
        else if (Event.current.button == 1)
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            list.Add(new FloatMenuOption("ResetBinding".Translate(), delegate
                        {
                        KeyCode keyCode = ((slot == KeyPrefs.BindingSlot.A) ? keyDef.defaultKeyCodeA : keyDef.defaultKeyCodeB);
                        KeyPrefs.KeyPrefsData.SetBinding(keyDef, slot, keyCode);
                        }));
            list.Add(new FloatMenuOption("ClearBinding".Translate(), delegate
                        {
                        KeyPrefs.KeyPrefsData.SetBinding(keyDef, slot, KeyCode.None);
                        }));
            Find.WindowStack.Add(new FloatMenu(list));
        }
    }

    // from Dialog_KeyBindings.cs
    private void DrawKeyEntry(KeyBindingDef keyDef, Rect parentRect, string label) {
        Rect rect = new Rect(parentRect.x, parentRect.y, parentRect.width, 34f);
//        GenUI.SetLabelAlign(TextAnchor.MiddleLeft);
        Widgets.Label(rect, label);
//        GenUI.ResetLabelAlign();
        float num = 4f;
        Vector2 vector = new Vector2(140f, 28f);
        Rect rect2 = new Rect(rect.x + rect.width - vector.x * 2f - num, rect.y, vector.x, vector.y);
        Rect rect3 = new Rect(rect.x + rect.width - vector.x, rect.y, vector.x, vector.y);
        string key = "BindingButtonToolTip";
        TooltipHandler.TipRegionByKey(rect2, key);
        TooltipHandler.TipRegionByKey(rect3, key);
        if (Widgets.ButtonText(rect2, KeyPrefs.KeyPrefsData.GetBoundKeyCode(keyDef, KeyPrefs.BindingSlot.A).ToStringReadable()))
        {
            SettingButtonClicked(keyDef, KeyPrefs.BindingSlot.A);
        }
        if (Widgets.ButtonText(rect3, KeyPrefs.KeyPrefsData.GetBoundKeyCode(keyDef, KeyPrefs.BindingSlot.B).ToStringReadable()))
        {
            SettingButtonClicked(keyDef, KeyPrefs.BindingSlot.B);
        }
    }

    void draw_log(Rect inRect){
        Listing_Standard l = new Listing_Standard();
        l.Begin(inRect);

        DrawKeyEntry(VDefOf.YADA_ToggleDebugLog, l.GetRect(34f), "Hotkey (default is \"§\")");
        l.CheckboxLabeled("Draw log overlay", ref Settings.drawLogOverlay);

        l.Label("X: " + Settings.logX); Settings.logX = (int)l.Slider(Settings.logX, 0, UI.screenWidth - 100);
        l.Label("Y: " + Settings.logY); Settings.logY = (int)l.Slider(Settings.logY, 0, UI.screenHeight - 100);
        l.Label("W: " + Settings.logW); Settings.logW = (int)l.Slider(Settings.logW, 100, UI.screenWidth);
        l.Label("H: " + Settings.logH); Settings.logH = (int)l.Slider(Settings.logH, 100, UI.screenHeight);
        l.Label("hue: " + Math.Round(Settings.logHue,2)); Settings.logHue = l.Slider(Settings.logHue, 0, 1);
        l.Label("opacity: " + Math.Round(Settings.logOpa,2)); Settings.logOpa = l.Slider(Settings.logOpa, 0, 1);
        l.Label("line spacing: " + Math.Round(Settings.logLineSpacing,2));
        Settings.logLineSpacing = l.Slider(Settings.logLineSpacing, 0.5f, 2);

        l.End();
    }

    public override string SettingsCategory() => "YADA";
    private int PageIndex = 0;
}
