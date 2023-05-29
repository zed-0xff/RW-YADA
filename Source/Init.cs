using HarmonyLib;
using Verse;

namespace YADA;

[StaticConstructorOnStartup]
public class Init {
    static Init() {
        DynamicPatch.PatchAll();
    }
}
