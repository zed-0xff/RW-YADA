using System;
using Verse;
using UnityEngine;

namespace zed_0xff.YADA;

public class YADASettings : ModSettings {
    public bool drawLogOverlay = false;

    public int logX = 0;
    public int logY = 0;
    public int logW = 1024;
    public int logH = 768;
    public float logHue = 0.5f;
    public float logOpa = 0.5f;
    public float logLineSpacing = 0.9f;

    public override void ExposeData() {
        Scribe_Values.Look(ref drawLogOverlay, "drawLogOverlay", false);
        Scribe_Values.Look(ref logX, "log.x", 0);
        Scribe_Values.Look(ref logY, "log.y", 0);
        Scribe_Values.Look(ref logW, "log.width", 1024);
        Scribe_Values.Look(ref logH, "log.height", 768);
        Scribe_Values.Look(ref logHue, "log.hue", 0.5f);
        Scribe_Values.Look(ref logOpa, "log.opacity", 0.5f);
        Scribe_Values.Look(ref logLineSpacing, "log.lineSpacing", 0.9f);
        base.ExposeData();
    }
}

public class ModConfig : Mod {

    public static string RootDir;

    public static YADASettings Settings { get; private set; }

    public ModConfig(ModContentPack content) : base(content) {
        RootDir = content.RootDir;
        Settings = GetSettings<YADASettings>();
    }

    public override void DoSettingsWindowContents(Rect inRect) {
        Listing_Standard l = new Listing_Standard();
        l.Begin(inRect);
        l.CheckboxLabeled("drawLogOverlay", ref Settings.drawLogOverlay);

        l.Label("log X: " + Settings.logX); Settings.logX = (int)l.Slider(Settings.logX, 0, UI.screenWidth - 100);
        l.Label("log Y: " + Settings.logY); Settings.logY = (int)l.Slider(Settings.logY, 0, UI.screenHeight - 100);
        l.Label("log W: " + Settings.logW); Settings.logW = (int)l.Slider(Settings.logW, 100, UI.screenWidth);
        l.Label("log H: " + Settings.logH); Settings.logH = (int)l.Slider(Settings.logH, 100, UI.screenHeight);
        l.Label("log hue: " + Math.Round(Settings.logHue,2)); Settings.logHue = l.Slider(Settings.logHue, 0, 1);
        l.Label("log opacity: " + Math.Round(Settings.logOpa,2)); Settings.logOpa = l.Slider(Settings.logOpa, 0, 1);
        l.Label("log line spacing: " + Math.Round(Settings.logLineSpacing,2));
        Settings.logLineSpacing = l.Slider(Settings.logLineSpacing, 0.5f, 2);

        l.End();
        base.DoSettingsWindowContents(inRect);
    }

    public override string SettingsCategory() => "YADA";

//    private static Vector2 scrollPosition = Vector2.zero;
//    private int PageIndex = 0;
}
