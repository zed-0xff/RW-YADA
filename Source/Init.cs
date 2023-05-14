using HarmonyLib;
using Verse;

namespace zed_0xff.YADA;

[StaticConstructorOnStartup]
public class Init {
    static Init() {
        DynamicPatch.PatchAll();
    }
}
