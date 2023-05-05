using System;
using System.Collections.Generic;
using Verse;
using UnityEngine;

namespace zed_0xff.YADA;

//public class YADASettings : ModSettings
//{
//    public override void ExposeData()
//    {
//        base.ExposeData();
//    }
//}

public class ModConfig : Mod {

    public static string RootDir;

    public ModConfig(ModContentPack content) : base(content) {
        RootDir = content.RootDir;
    }

//    public static YADASettings Settings { get; private set; }

    public override string SettingsCategory() => "YADA";

    private static Vector2 scrollPosition = Vector2.zero;
    private int PageIndex = 0;
}
