using HarmonyLib;
using Verse;

namespace zed_0xff.YADA;

[StaticConstructorOnStartup]
public class Init
{
    static Init() {
        Harmony harmony = new Harmony("zed_0xff.YADA");
        harmony.PatchAll();
        PatchDynamic.PatchAll();
    }
}
